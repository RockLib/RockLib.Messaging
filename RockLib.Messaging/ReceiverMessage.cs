using RockLib.Compression;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging
{
    /// <summary>
    /// A base class for implementations of the <see cref="IReceiverMessage"/> interface
    /// that handles binary/string encoding, compression, and provides a hook for header
    /// initialization.
    /// </summary>
    public abstract class ReceiverMessage : IReceiverMessage, IDisposable
    {
        private static readonly GZipDecompressor _gzip = new();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly Lazy<string> _stringPayload;
        private readonly Lazy<byte[]> _binaryPayload;
        private readonly Lazy<HeaderDictionary> _headers;

        private string? _handledBy;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiverMessage"/> class.
        /// </summary>
        /// <param name="getRawPayload">
        /// A function that returns the raw payload. Note that the <see cref="Payload"/> struct
        /// is implicitly convertable from the string and byte array types (i.e. this function
        /// can just return a string or byte array).
        /// </param>
        protected ReceiverMessage(Func<Payload> getRawPayload)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(getRawPayload);
#else
            if (getRawPayload is null) { throw new ArgumentNullException(nameof(getRawPayload)); }
#endif

            var rawPayload = new Lazy<object>(() => getRawPayload().Value
                ?? throw new InvalidOperationException("Cannot decode/decompress null payload."));

            _stringPayload = new Lazy<string>(() =>
            {
                if (rawPayload.Value is string stringPayload)
                {
                    if (this.IsCompressed())
                    {
                        if (this.IsBinary())
                        {
                            return Convert.ToBase64String(_gzip.Decompress(Convert.FromBase64String(stringPayload)));
                        }
                        return Encoding.UTF8.GetString(_gzip.Decompress(Convert.FromBase64String(stringPayload)));
                    }
                    return stringPayload;
                }
                if (rawPayload.Value is byte[] binaryPayload)
                {
                    if (this.IsCompressed())
                    {
                        binaryPayload = _gzip.Decompress(binaryPayload);
                    }
                    if (this.IsBinary())
                    {
                        return Convert.ToBase64String(binaryPayload);
                    }
                    return Encoding.UTF8.GetString(binaryPayload);
                }
                throw new InvalidOperationException($"Unknown payload type: {rawPayload.GetType().FullName}");
            });

            _binaryPayload = new Lazy<byte[]>(() =>
            {
                if (rawPayload.Value is string stringPayload)
                {
                    if (this.IsCompressed())
                    {
                        return _gzip.Decompress(Convert.FromBase64String(stringPayload));
                    }
                    if (this.IsBinary())
                    {
                        return Convert.FromBase64String(stringPayload);
                    }
                    return Encoding.UTF8.GetBytes(stringPayload);
                }
                if (rawPayload.Value is byte[] binaryPayload)
                {
                    if (this.IsCompressed())
                    {
                        return _gzip.Decompress(binaryPayload);
                    }
                    return binaryPayload;
                }
                throw new InvalidOperationException($"Unknown payload type: {rawPayload.GetType().FullName}");
            });

            _headers = new Lazy<HeaderDictionary>(() =>
            {
                var headers = new Dictionary<string, object>();
                InitializeHeaders(headers);
                return new HeaderDictionary(headers);
            });
        }

        /// <summary>
        /// Gets the payload of the message as a string.
        /// </summary>
        public string StringPayload => _stringPayload.Value;

        /// <summary>
        /// Gets the payload of the message as a byte array.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] BinaryPayload => _binaryPayload.Value;
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets the headers of the message.
        /// </summary>
        public HeaderDictionary Headers => _headers.Value;

        /// <summary>
        /// Gets a value indicating whether this message has been handled by one of the
        /// <see cref="AcknowledgeAsync"/>, <see cref="RollbackAsync"/> or <see cref="RejectAsync"/>
        /// methods.
        /// </summary>
        public bool Handled => _handledBy is not null;

        /// <summary>
        /// Indicates that the message was successfully processed and should not
        /// be redelivered.
        /// </summary>
        public async Task AcknowledgeAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                await AcknowledgeMessageAsync(cancellationToken).ConfigureAwait(false);
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Indicates that the message was not successfully processed but should be
        /// (or should be allowed to be) redelivered.
        /// </summary>
        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                await RollbackMessageAsync(cancellationToken).ConfigureAwait(false);
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Indicates that the message could not be successfully processed and should
        /// not be redelivered.
        /// </summary>
        public async Task RejectAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                await RejectMessageAsync(cancellationToken).ConfigureAwait(false);
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void ThrowIfHandled([CallerMemberName] string? callerMemberName = null)
        {
            if (Handled)
            {
                throw new InvalidOperationException($"Cannot {callerMemberName} message: the message has already been handled by {_handledBy}.");
            }
        }

        private void SetHandled([CallerMemberName] string? callerMemberName = null)
        {
            _handledBy = callerMemberName;
        }

        /// <summary>
        /// When overridden in a derived class, initializes the <paramref name="headers"/>
        /// parameter with the headers for this receiver message.
        /// </summary>
        /// <param name="headers">
        /// A dictionary to be filled with the headers for this receiver message.
        /// </param>
        protected abstract void InitializeHeaders(IDictionary<string, object> headers);

        /// <summary>
        /// When overridden in a derived class, indicates that the message was
        /// successfully processed and should not be redelivered.
        /// </summary>
        /// <remarks>
        /// If the concept of acknowledging a message doesn't make sense for an derived
        /// class, then the overriden method should do nothing instead of throwing a
        /// <see cref="NotImplementedException"/> or similar exception.
        /// </remarks>
        protected abstract Task AcknowledgeMessageAsync(CancellationToken cancellationToken);

        /// <summary>
        /// When overridden in a derived class, indicates that the message was not
        /// successfully processed but should be (or should be allowed to be) redelivered.
        /// </summary>
        /// <remarks>
        /// If the concept of rolling back a message doesn't make sense for an derived
        /// class, then the overriden method should do nothing instead of throwing a
        /// <see cref="NotImplementedException"/> or similar exception.
        /// </remarks>
        protected abstract Task RollbackMessageAsync(CancellationToken cancellationToken);

        /// <summary>
        /// When overridden in a derived class, indicates that the message could not be
        /// successfully processed and should not be redelivered.
        /// </summary>
        /// <remarks>
        /// If the concept of rejecting a message doesn't make sense for an derived
        /// class, then the overriden method should do nothing instead of throwing a
        /// <see cref="NotImplementedException"/> or similar exception.
        /// </remarks>
        protected abstract Task RejectMessageAsync(CancellationToken cancellationToken);

        /// <summary>
        /// A struct that contains a payload value. Implicitly convertable from the
        /// string and byte array types.
        /// </summary>
        protected struct Payload
            : IEquatable<Payload>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Payload"/> class with a string value.
            /// </summary>
            public Payload(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));

            /// <summary>
            /// Initializes a new instance of the <see cref="Payload"/> class with a binary value.
            /// </summary>
            public Payload(byte[] value) => Value = value ?? throw new ArgumentNullException(nameof(value));

            /// <summary>
            /// The value of the payload.
            /// </summary>
            public object Value { get; }

            /// <summary>
            /// Gets a hash code based on the <see cref="Value"/>.
            /// </summary>
            /// <returns>A hash code value</returns>
            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            /// <summary>
            /// Checks to see if the given object is equal to the current <see cref="Payload" /> instance.
            /// </summary>
            /// <param name="obj">The object to check for equality.</param>
            /// <returns>Returns <c>true</c> if the objects are equals; otherwise, <c>false</c>.</returns>
            public override bool Equals(object? obj)
            {
                var areEqual = false;

                if (obj is Payload payload)
                {
                    areEqual = Equals(payload);
                }

                return areEqual;
            }

            /// <summary>
            /// Provides a type-safe equality check.
            /// </summary>
            /// <param name="other">The object to check for equality.</param>
            /// <returns>Returns <c>true</c> if the objects are equals; otherwise, <c>false</c>.</returns>
            public bool Equals(Payload other)
            {
                return Value == other.Value;
            }

            /// <summary>
            /// Determines whether two specified <see cref="Payload" /> objects have the same value. 
            /// </summary>
            /// <param name="a">A <see cref="Payload" /> or a null reference.</param>
            /// <param name="b">A <see cref="Payload" /> or a null reference.</param>
            /// <returns><b>true</b> if the value of <paramref name="a"/> is the same as the value of <paramref name="b"/>; otherwise, <b>false</b>. </returns>
            public static bool operator ==(Payload a, Payload b)
            {
                return a.Equals(b);
            }

            /// <summary>
            /// Determines whether two specified <see cref="Payload" /> objects have different value. 
            /// </summary>
            /// <param name="a">A <see cref="Payload" /> or a null reference.</param>
            /// <param name="b">A <see cref="Payload" /> or a null reference.</param>
            /// <returns><b>true</b> if the value of <paramref name="a"/> is different from the value of <paramref name="b"/>; otherwise, <b>false</b>. </returns>
            public static bool operator !=(Payload a, Payload b)
            {
                return !(a == b);
            }

            /// <summary>
            /// Converts a string value to a <see cref="Payload"/> struct.
            /// </summary>
#pragma warning disable CA2225 // Operator overloads have named alternates
            public static implicit operator Payload(string value) => new Payload(value);

            /// <summary>
            /// Converts a binary value to a <see cref="Payload"/> struct.
            /// </summary>
            public static implicit operator Payload(byte[] value) => new Payload(value);
#pragma warning restore CA2225 // Operator overloads have named alternates
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        /// <param name="disposing">Set to <c>true</c> if this is called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _semaphore.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
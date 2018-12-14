using RockLib.Compression;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RockLib.Messaging
{
    /// <summary>
    /// A base class for implementations of the <see cref="IReceiverMessage"/> interface
    /// that handles binary/string encoding, compression, and provides a hook for header
    /// initialization.
    /// </summary>
    public abstract class ReceiverMessage : IReceiverMessage
    {
        private static readonly GZipDecompressor _gzip = new GZipDecompressor();

        private readonly Lazy<string> _stringPayload;
        private readonly Lazy<byte[]> _binaryPayload;
        private readonly Lazy<HeaderDictionary> _headers;

        private string _handledBy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiverMessage"/> class using
        /// a raw string payload.
        /// </summary>
        /// <param name="getRawPayload">
        /// A function that returns the raw payload. Note that the <see cref="Payload"/> struct
        /// is implicitly convertable from the string and byte array types (i.e. this function
        /// can just return a string or byte array).
        /// </param>
        protected ReceiverMessage(Func<Payload> getRawPayload)
        {
            if (getRawPayload == null)
                throw new ArgumentNullException(nameof(getRawPayload));

            var rawPayload = new Lazy<object>(() => getRawPayload().Value
                ?? throw new InvalidOperationException("Cannot decode/decompress null payload."));

            _stringPayload = new Lazy<string>(() =>
            {
                if (rawPayload.Value is string stringPayload)
                {
                    if (this.IsCompressed())
                    {
                        if (this.IsBinary())
                            return Convert.ToBase64String(_gzip.Decompress(Convert.FromBase64String(stringPayload)));
                        return Encoding.UTF8.GetString(_gzip.Decompress(Convert.FromBase64String(stringPayload)));
                    }
                    return stringPayload;
                }
                if (rawPayload.Value is byte[] binaryPayload)
                {
                    if (this.IsCompressed())
                        binaryPayload = _gzip.Decompress(binaryPayload);
                    if (this.IsBinary())
                        return Convert.ToBase64String(binaryPayload);
                    return Encoding.UTF8.GetString(binaryPayload);
                }
                throw new InvalidOperationException($"Unknown payload type: {rawPayload.GetType().FullName}");
            });

            _binaryPayload = new Lazy<byte[]>(() =>
            {
                if (rawPayload.Value is string stringPayload)
                {
                    if (this.IsCompressed())
                        return _gzip.Decompress(Convert.FromBase64String(stringPayload));
                    if (this.IsBinary())
                        return Convert.FromBase64String(stringPayload);
                    return Encoding.UTF8.GetBytes(stringPayload);
                }
                if (rawPayload.Value is byte[] binaryPayload)
                {
                    if (this.IsCompressed())
                        return _gzip.Decompress(binaryPayload);
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
        public byte[] BinaryPayload => _binaryPayload.Value;

        /// <summary>
        /// Gets the headers of the message.
        /// </summary>
        public HeaderDictionary Headers => _headers.Value;

        /// <summary>
        /// Gets a value indicating whether this message has been handled by one of the
        /// <see cref="Acknowledge"/>, <see cref="Rollback"/> or <see cref="Reject"/>
        /// methods.
        /// </summary>
        public bool Handled => _handledBy != null;

        /// <summary>
        /// Indicates that the message was successfully processed and should not
        /// be redelivered.
        /// </summary>
        public void Acknowledge()
        {
            lock (this)
            {
                ThrowIfHandled();
                AcknowledgeMessage();
                SetHandled();
            }
        }

        /// <summary>
        /// Indicates that the message was not successfully processed but should be
        /// (or should be allowed to be) redelivered.
        /// </summary>
        public void Rollback()
        {
            lock (this)
            {
                ThrowIfHandled();
                RollbackMessage();
                SetHandled();
            }
        }

        /// <summary>
        /// Indicates that the message could not be successfully processed and should
        /// not be redelivered.
        /// </summary>
        public void Reject()
        {
            lock (this)
            {
                ThrowIfHandled();
                RejectMessage();
                SetHandled();
            }
        }

        private void ThrowIfHandled([CallerMemberName] string callerMemberName = null)
        {
            if (Handled)
                throw new InvalidOperationException($"Cannot {callerMemberName} message: the message has already been handled by {_handledBy}.");
        }

        private void SetHandled([CallerMemberName] string callerMemberName = null)
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
        protected abstract void AcknowledgeMessage();

        /// <summary>
        /// When overridden in a derived class, indicates that the message was not
        /// successfully processed but should be (or should be allowed to be) redelivered.
        /// </summary>
        /// <remarks>
        /// If the concept of rolling back a message doesn't make sense for an derived
        /// class, then the overriden method should do nothing instead of throwing a
        /// <see cref="NotImplementedException"/> or similar exception.
        /// </remarks>
        protected abstract void RollbackMessage();

        /// <summary>
        /// When overridden in a derived class, indicates that the message could not be
        /// successfully processed and should not be redelivered.
        /// </summary>
        /// <remarks>
        /// If the concept of rejecting a message doesn't make sense for an derived
        /// class, then the overriden method should do nothing instead of throwing a
        /// <see cref="NotImplementedException"/> or similar exception.
        /// </remarks>
        protected abstract void RejectMessage();

        /// <summary>
        /// A struct that contains a payload value. Implicitly convertable from the
        /// string and byte array types.
        /// </summary>
        protected struct Payload
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
            public readonly object Value;

            /// <summary>
            /// Converts a string value to a <see cref="Payload"/> struct.
            /// </summary>
            public static implicit operator Payload(string value) => new Payload(value);

            /// <summary>
            /// Converts a binary value to a <see cref="Payload"/> struct.
            /// </summary>
            public static implicit operator Payload(byte[] value) => new Payload(value);
        }
    }
}
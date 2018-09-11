using RockLib.Compression;
using System;
using System.Collections.Generic;
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

            _stringPayload = new Lazy<string>(() =>
            {
                var rawPayload = getRawPayload().Value
                    ?? throw new InvalidOperationException("Cannot decode/decompress null payload.");

                if (rawPayload is string stringPayload)
                {
                    if (this.IsCompressed())
                    {
                        if (this.IsBinary())
                            return Convert.ToBase64String(_gzip.Decompress(Convert.FromBase64String(stringPayload)));
                        return Encoding.UTF8.GetString(_gzip.Decompress(Convert.FromBase64String(stringPayload)));
                    }

                    return stringPayload;
                }
                if (rawPayload is byte[] binaryPayload)
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
                var rawPayload = getRawPayload().Value
                    ?? throw new InvalidOperationException("Cannot decode/decompress null payload.");

                if (rawPayload is string stringPayload)
                {
                    if (this.IsCompressed())
                        return _gzip.Decompress(Convert.FromBase64String(stringPayload));
                    if (this.IsBinary())
                        return Convert.FromBase64String(stringPayload);
                    return Encoding.UTF8.GetBytes(stringPayload);
                }
                if (rawPayload is byte[] binaryPayload)
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
        /// Gets the priority of the message, if applicable.
        /// </summary>
        public abstract byte? Priority { get; }

        /// <summary>
        /// Gets a value indicating whether the message is transactional. If true,
        /// either the <see cref="Acknowledge"/> or <see cref="Rollback"/> method must
        /// be called when processing the message.
        /// </summary>
        public abstract bool IsTransactional { get; }

        /// <summary>
        /// If <see cref="IsTransactional"/> is true, communicate to the server that
        /// the message was successfully processed and should not be redelivered.
        /// </summary>
        public abstract void Acknowledge();

        /// <summary>
        /// If <see cref="IsTransactional"/> is true, communicate to the server that
        /// the message was not successfully processed and should be redelivered.
        /// </summary>
        public abstract void Rollback();

        /// <summary>
        /// Initialize the <paramref name="headers"/> parameter with the headers
        /// for this receiver message.
        /// </summary>
        /// <param name="headers">
        /// A dictionary to be filled with the headers for this receiver message.
        /// </param>
        protected abstract void InitializeHeaders(IDictionary<string, object> headers);

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
            /// <param name="value"></param>
            public static implicit operator Payload(byte[] value) => new Payload(value);
        }
    }
}
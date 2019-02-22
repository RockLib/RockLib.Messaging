using RockLib.Compression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines an outgoing message.
    /// </summary>
    public sealed class SenderMessage
    {
        private static readonly GZipCompressor _gzip = new GZipCompressor();

        private readonly Lazy<string> _stringPayload;
        private readonly Lazy<byte[]> _binaryPayload;
        private readonly IDictionary<string, object> _headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SenderMessage"/> class.
        /// </summary>
        /// <param name="payload">The payload of the message.</param>
        /// <param name="compress">Whether to compress the payload of the message.</param>
        /// <param name="validateHeaderValue">
        /// A function that validates header values, returning either the value passed to it
        /// or an equivalent value. If a value is invalid, the function should attempt to
        /// convert it to another type that is valid. If a value cannot be converted, the
        /// function should throw an exception.
        /// </param>
        public SenderMessage(string payload, bool compress = false, Func<object, object> validateHeaderValue = null)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            _headers = new HeaderDictionary(validateHeaderValue);
            InitAsString(payload, compress, out _stringPayload, out _binaryPayload);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SenderMessage"/> class.
        /// </summary>
        /// <param name="payload">The payload of the message.</param>
        /// <param name="compress">Whether to compress the payload of the message.</param>
        /// <param name="validateHeaderValue">
        /// A function that validates header values, returning either the value passed to it
        /// or an equivalent value. If a value is invalid, the function should attempt to
        /// convert it to another type that is valid. If a value cannot be converted, the
        /// function should throw an exception.
        /// </param>
        public SenderMessage(byte[] payload, bool compress = false, Func<object, object> validateHeaderValue = null)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            _headers = new HeaderDictionary(validateHeaderValue, isBinary: true);
            InitAsBinary(payload, compress, out _stringPayload, out _binaryPayload);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SenderMessage"/> as a copy of
        /// the specified <see cref="IReceiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">The <see cref="IReceiverMessage"/> to make a copy of.</param>
        /// <param name="validateHeaderValue">
        /// A function that validates header values, returning either the value passed to it
        /// or an equivalent value. If a value is invalid, the function should attempt to
        /// convert it to another type that is valid. If a value cannot be converted, the
        /// function should throw an exception.
        /// </param>
        public SenderMessage(IReceiverMessage receiverMessage, Func<object, object> validateHeaderValue = null)
        {
            if (receiverMessage == null)
                throw new ArgumentNullException(nameof(receiverMessage));

            _headers = new HeaderDictionary(validateHeaderValue);

            if (receiverMessage.IsBinary())
                InitAsBinary(receiverMessage.BinaryPayload, receiverMessage.IsCompressed(), out _stringPayload, out _binaryPayload);
            else
                InitAsString(receiverMessage.StringPayload, receiverMessage.IsCompressed(), out _stringPayload, out _binaryPayload);

            foreach (var header in receiverMessage.Headers)
                if (header.Key != HeaderNames.MessageId) // Don't copy the message id
                    _headers[header.Key] = header.Value;
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
        /// Gets or sets the headers of the message.
        /// </summary>
        public IDictionary<string, object> Headers
        {
            get => _headers;
            set
            {
                _headers.Clear();
                if (value != null)
                    foreach (var header in value)
                        _headers.Add(header);
            }
        }

        /// <summary>
        /// Gets the ID of the message.
        /// </summary>
        public string MessageId => Headers.TryGetValue(HeaderNames.MessageId, out var value)
            && value is string messageId
                ? messageId
                : null;

        /// <summary>
        /// Gets a value indicating whether the message is compressed.
        /// </summary>
        public bool IsCompressed =>
            Headers.TryGetValue(HeaderNames.IsCompressedPayload, out var value)
                && ((value is bool isCompressed && isCompressed)
                    || value is string isCompressedString && isCompressedString.ToLowerInvariant() == "true");

        /// <summary>
        /// Gets a value indicating whether the message was constructed with a byte array
        /// payload. False indicates that the message was constructed with a string payload.
        /// </summary>
        public bool IsBinary =>
            Headers.TryGetValue(HeaderNames.IsBinaryPayload, out var value)
                && ((value is bool isBinary && isBinary)
                    || value is string isBinaryString && isBinaryString.ToLowerInvariant() == "true");

        /// <summary>
        /// Gets or sets the originating system of the message.
        /// </summary>
        public string OriginatingSystem
        {
            get => Headers.TryGetValue(HeaderNames.OriginatingSystem, out var value) && value is string originatingSystem
                ? originatingSystem
                : null;
            set => Headers[HeaderNames.OriginatingSystem] = value;
        }

        private void InitAsString(string payload, bool compress, out Lazy<string> stringPayload, out Lazy<byte[]> binaryPayload)
        {
            stringPayload = new Lazy<string>(() => payload);
            binaryPayload = new Lazy<byte[]>(() => Encoding.UTF8.GetBytes(payload));

            if (compress)
                Compress(ref stringPayload, ref binaryPayload);
        }

        private void InitAsBinary(byte[] payload, bool compress, out Lazy<string> stringPayload, out Lazy<byte[]> binaryPayload)
        {
            stringPayload = new Lazy<string>(() => Convert.ToBase64String(payload));
            binaryPayload = new Lazy<byte[]>(() => payload);

            if (compress)
                Compress(ref stringPayload, ref binaryPayload);
        }

        private void Compress(ref Lazy<string> stringPayload, ref Lazy<byte[]> binaryPayload)
        {
            var uncompressedPayload = binaryPayload;
            var compressedPayload = new Lazy<byte[]>(() => _gzip.Compress(uncompressedPayload.Value));

            if (compressedPayload.Value.Length < uncompressedPayload.Value.Length)
            {
                stringPayload = new Lazy<string>(() => Convert.ToBase64String(compressedPayload.Value));
                binaryPayload = compressedPayload;
                _headers[HeaderNames.IsCompressedPayload] = true;
            }
        }

        private static object DefaultValidateHeaderValue(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            if (value is bool b)
                return b ? "true" : "false";
            if (value is string || value is decimal || value.GetType().GetTypeInfo().IsPrimitive)
                return value;
            if (value is Enum e)
                return e.ToString();
            if (value is DateTime dateTime)
                return dateTime.ToString("O");
            if (value is TimeSpan timeSpan)
                return timeSpan.ToString("c");
            if (value is Guid guid)
                return guid.ToString("D");
            if (value is Uri uri)
                return uri.ToString();
            if (value is DateTimeOffset dateTimeOffset)
                return dateTimeOffset.ToString("O");
            throw new ArgumentException("Header value must be primitive or enum type or one of: String, Decimal, DateTime, TimeSpan, Guid, Uri, or DateTimeOffset.", nameof(value));
        }

        private class HeaderDictionary : IDictionary<string, object>
        {
            private readonly IDictionary<string, object> _headers = new Dictionary<string, object>();

            /// <summary>
            /// Initializes a new instance of the <see cref="HeaderDictionary"/> class and
            /// sets the value of the <see cref="HeaderNames.MessageId"/> header to a new
            /// <see cref="Guid"/>. If the <paramref name="isBinary"/> parameter is true,
            /// then the <see cref="HeaderNames.IsBinaryPayload"/> header is set to true.
            /// </summary>
            public HeaderDictionary(Func<object, object> validateValue, bool isBinary = false)
            {
                ValidateValue = validateValue ?? DefaultValidateHeaderValue;
                this[HeaderNames.MessageId] = Guid.NewGuid();
                if (isBinary)
                    this[HeaderNames.IsBinaryPayload] = true;
            }

            public void Clear()
            {
                var messageId = this[HeaderNames.MessageId];
                _headers.Clear();
                this[HeaderNames.MessageId] = messageId;
            }

            public object this[string key]
            {
                get => _headers[key];
                set => _headers[key] = ValidateValue(value);
            }

            public void Add(string key, object value) =>
                _headers.Add(key, ValidateValue(value));

            public void Add(KeyValuePair<string, object> item) =>
                _headers.Add(new KeyValuePair<string, object>(item.Key, ValidateValue(item.Value)));

            public Func<object, object> ValidateValue { get; }

            public ICollection<string> Keys => _headers.Keys;
            public ICollection<object> Values => _headers.Values;
            public int Count => _headers.Count;
            public bool IsReadOnly => _headers.IsReadOnly;
            public bool Contains(KeyValuePair<string, object> item) => _headers.Contains(item);
            public bool ContainsKey(string key) => _headers.ContainsKey(key);
            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _headers.CopyTo(array, arrayIndex);
            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _headers.GetEnumerator();
            public bool Remove(string key) => _headers.Remove(key);
            public bool Remove(KeyValuePair<string, object> item) => _headers.Remove(item);
            public bool TryGetValue(string key, out object value) => _headers.TryGetValue(key, out value);
            IEnumerator IEnumerable.GetEnumerator() => _headers.GetEnumerator();
        }
    }
}
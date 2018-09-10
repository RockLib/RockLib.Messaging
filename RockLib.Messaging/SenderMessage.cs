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
        /// <param name="priority">The priority of the message.</param>
        /// <param name="compress">Whether to compress the payload of the message.</param>
        /// <param name="validateHeaderValue">
        /// A function that validates header values, returning either the value passed to it
        /// or an equivalent value. If a value is invalid, the function should attempt to
        /// convert it to another type that is valid. If a value cannot be converted, the
        /// function should throw an exception.
        /// </param>
        public SenderMessage(string payload, byte? priority = null, bool compress = false, Func<object, object> validateHeaderValue = null)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            _headers = new ValidatingDictionary(validateHeaderValue ?? DefaultValidateHeaderValue)
            {
                [HeaderNames.MessageId] = Guid.NewGuid()
            };

            _stringPayload = new Lazy<string>(() => payload);
            _binaryPayload = new Lazy<byte[]>(() => Encoding.UTF8.GetBytes(payload));

            if (compress)
                Compress(ref _binaryPayload, ref _stringPayload);

            Priority = priority;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SenderMessage"/> class.
        /// </summary>
        /// <param name="payload">The payload of the message.</param>
        /// <param name="priority">The priority of the message.</param>
        /// <param name="compress">Whether to compress the payload of the message.</param>
        /// <param name="validateHeaderValue">
        /// A function that validates header values, returning either the value passed to it
        /// or an equivalent value. If a value is invalid, the function should attempt to
        /// convert it to another type that is valid. If a value cannot be converted, the
        /// function should throw an exception.
        /// </param>
        public SenderMessage(byte[] payload, byte? priority = null, bool compress = false, Func<object, object> validateHeaderValue = null)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            _headers = new ValidatingDictionary(validateHeaderValue ?? DefaultValidateHeaderValue)
            {
                [HeaderNames.MessageId] = Guid.NewGuid(),
                [HeaderNames.IsBinaryMessage] = true
            };

            _stringPayload = new Lazy<string>(() => Convert.ToBase64String(payload));
            _binaryPayload = new Lazy<byte[]>(() => payload);

            if (compress)
                Compress(ref _binaryPayload, ref _stringPayload);

            Priority = priority;
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
        /// Gets the priority of the message.
        /// </summary>
        public byte? Priority { get; }

        /// <summary>
        /// Gets the ID of the message.
        /// </summary>
        public Guid MessageId => (Guid)Headers[HeaderNames.MessageId];

        /// <summary>
        /// Gets a value indicating whether the message is compressed.
        /// </summary>
        public bool IsCompressed =>
            Headers.TryGetValue(HeaderNames.CompressedPayload, out var value) && value is bool isCompressed && isCompressed;

        /// <summary>
        /// Gets a value indicating whether the message was constructed with a byte array
        /// payload. False indicates that the message was constructed with a string payload.
        /// </summary>
        public bool IsBinary =>
            Headers.TryGetValue(HeaderNames.IsBinaryMessage, out var value) && value is bool isBinary && isBinary;

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

        private void Compress(
            ref Lazy<byte[]> _binaryPayload,
            ref Lazy<string> _stringPayload)
        {
            var uncompressedPayload = _binaryPayload;
            var compressedPayload = new Lazy<byte[]>(() => _gzip.Compress(uncompressedPayload.Value));

            if (compressedPayload.Value.Length < uncompressedPayload.Value.Length)
            {
                _binaryPayload = compressedPayload;
                _stringPayload = new Lazy<string>(() => Convert.ToBase64String(compressedPayload.Value));
                _headers[HeaderNames.CompressedPayload] = true;
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
            if (value is DateTime dateTime)
                return dateTime.ToString("O");
            if (value is Guid guid)
                return guid.ToString("D");
            if (value is DateTimeOffset dateTimeOffset)
                return dateTimeOffset.ToString("O");
            throw new ArgumentException("Value must be primitive type or one of: String, DateTime, Guid, or DateTimeOffset.", nameof(value));
        }

        private class ValidatingDictionary : IDictionary<string, object>
        {
            private readonly IDictionary<string, object> _headers = new Dictionary<string, object>();

            public ValidatingDictionary(Func<object, object> validateValue)
            {
                ValidateValue = validateValue;
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
            public void Clear() => _headers.Clear();
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
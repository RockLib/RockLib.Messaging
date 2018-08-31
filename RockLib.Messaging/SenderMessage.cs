using RockLib.Compression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RockLib.Messaging
{
    public sealed class SenderMessage
    {
        private static readonly GZipCompressor _compressor = new GZipCompressor();

        private readonly Lazy<string> _stringPayload;
        private readonly Lazy<byte[]> _binaryPayload;
        private readonly IDictionary<string, object> _headers;

        public SenderMessage(string payload, byte? priority = null, bool compress = false, Action<object> validateHeaderValue = null)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            if (compress)
            {
                _binaryPayload = new Lazy<byte[]>(() => _compressor.Compress(Encoding.UTF8.GetBytes(payload)));
                _stringPayload = new Lazy<string>(() => Convert.ToBase64String(_binaryPayload.Value));
            }
            else
            {
                _stringPayload = new Lazy<string>(() => payload);
                _binaryPayload = new Lazy<byte[]>(() => Encoding.UTF8.GetBytes(payload));
            }

            Priority = priority;
            Compressed = compress;
            _headers = new HeaderDictionary(validateHeaderValue ?? ValidateHeaderValue);
        }

        public SenderMessage(byte[] payload, byte? priority = null, bool compress = false, Action<object> validateHeaderValue = null)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            if (compress)
            {
                _binaryPayload = new Lazy<byte[]>(() => _compressor.Compress(payload));
                _stringPayload = new Lazy<string>(() => Convert.ToBase64String(_binaryPayload.Value));
            }
            else
            {
                _stringPayload = new Lazy<string>(() => Convert.ToBase64String(payload));
                _binaryPayload = new Lazy<byte[]>(() => payload);
            }

            Priority = priority;
            Compressed = compress;
            _headers = new HeaderDictionary(validateHeaderValue ?? ValidateHeaderValue);
        }

        public string StringPayload => _stringPayload.Value;

        public byte[] BinaryPayload => _binaryPayload.Value;

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

        public byte? Priority { get; }

        public bool Compressed { get; }

        protected static void ValidateHeaderValue(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            if (!(value is string || value.GetType().GetTypeInfo().IsPrimitive))
                throw new ArgumentException("Value must be string or primitive.", nameof(value));
        }

        private class HeaderDictionary : IDictionary<string, object>
        {
            private readonly IDictionary<string, object> _headers = new Dictionary<string, object>();

            public HeaderDictionary(Action<object> validate)
            {
                Validate = validate;
            }

            public Action<object> Validate { get; }

            public object this[string key]
            {
                get => _headers[key];
                set
                {
                    Validate(value);
                    _headers[key] = value;
                }
            }

            public void Add(string key, object value)
            {
                Validate(value);
                _headers.Add(key, value);
            }

            public void Add(KeyValuePair<string, object> item)
            {
                Validate(item.Value);
                _headers.Add(item);
            }

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
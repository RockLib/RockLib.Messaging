using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace RockLib.Messaging
{
    public sealed class HeaderDictionary : IReadOnlyDictionary<string, object>
    {
        private readonly IReadOnlyDictionary<string, object> _headers;

        public HeaderDictionary(IReadOnlyDictionary<string, object> headers) =>
            _headers = headers ?? throw new ArgumentNullException(nameof(headers));

        public bool TryGetStringValue(string key, out string value)
        {
            if (_headers.TryGetValue(key, out var objectValue))
            {
                if (objectValue == null)
                {
                    value = null;
                    return true;
                }

                if (objectValue is string stringValue)
                {
                    value = stringValue;
                    return true;
                }

                value = objectValue.ToString();
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGetInt32Value(string key, out int value)
        {
            if (_headers.TryGetValue(key, out var objectValue))
            {
                if (objectValue == null)
                {
                    value = 0;
                    return false;
                }

                if (objectValue is int int32Value)
                {
                    value = int32Value;
                    return true;
                }

                var converter = TypeDescriptor.GetConverter(objectValue);
                if (converter.CanConvertTo(typeof(int)))
                {
                    value = (int)converter.ConvertTo(objectValue, typeof(int));
                    return true;
                }
            }

            value = 0;
            return false;
        }

        public bool TryGetBooleanValue(string key, out bool value)
        {
            if (_headers.TryGetValue(key, out var objectValue))
            {
                if (objectValue == null)
                {
                    value = false;
                    return false;
                }

                if (objectValue is bool boolValue)
                {
                    value = boolValue;
                    return true;
                }

                if (objectValue is string stringValue)
                {
                    switch (stringValue.ToLowerInvariant())
                    {
                        case "true":
                            value = true;
                            return true;
                        case "false":
                            value = false;
                            return true;
                    }
                }

                var converter = TypeDescriptor.GetConverter(objectValue);
                if (converter.CanConvertTo(typeof(bool)))
                {
                    value = (bool)converter.ConvertTo(objectValue, typeof(bool));
                    return true;
                }
            }

            value = false;
            return false;
        }

        public object this[string key] => _headers[key];
        public IEnumerable<string> Keys => _headers.Keys;
        public IEnumerable<object> Values => _headers.Values;
        public int Count => _headers.Count;
        public bool ContainsKey(string key) => _headers.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _headers.GetEnumerator();
        public bool TryGetValue(string key, out object value) => _headers.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => _headers.GetEnumerator();
    }
}
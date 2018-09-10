using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace RockLib.Messaging
{
    /// <summary>
    /// Represents the headers of a received message.
    /// </summary>
    public sealed class HeaderDictionary : IReadOnlyDictionary<string, object>
    {
        private readonly IReadOnlyDictionary<string, object> _headers;

        /// <summary>
        /// Initialize a new instance of the <see cref="HeaderDictionary"/> class.
        /// </summary>
        /// <param name="headers">A dictionary that contains the actual header values.</param>
        public HeaderDictionary(IReadOnlyDictionary<string, object> headers) =>
            _headers = headers ?? throw new ArgumentNullException(nameof(headers));

        /// <summary>
        /// Gets the value that is associated with the specified header name as a
        /// <see cref="string"/>.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified header name,
        /// if the header name is found and its value can be converted to a <see cref="string"/>;
        /// otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// True if the dictionary contains a header with the specified name and its value
        /// can be converted to a <see cref="string"/>; otherwise false.
        /// </returns>
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

        /// <summary>
        /// Gets the value that is associated with the specified header name as an
        /// <see cref="int"/>.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified header name,
        /// if the header name is found and its value can be converted to an <see cref="int"/>;
        /// otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// True if the dictionary contains a header with the specified name and its value
        /// can be converted to a <see cref="int"/>; otherwise false.
        /// </returns>
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

        /// <summary>
        /// Gets the value that is associated with the specified header name as an
        /// <see cref="bool"/>.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified header name,
        /// if the header name is found and its value can be converted to an <see cref="bool"/>;
        /// otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// True if the dictionary contains a header with the specified name and its value
        /// can be converted to a <see cref="bool"/>; otherwise false.
        /// </returns>
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

        /// <summary>
        /// Gets the header value with the specified name.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <returns>The header value.</returns>
        public object this[string key] => _headers[key];

        /// <summary>
        /// Gets a collection representing the header names.
        /// </summary>
        public IEnumerable<string> Keys => _headers.Keys;

        /// <summary>
        /// Gets a collection representing the header values.
        /// </summary>
        public IEnumerable<object> Values => _headers.Values;

        /// <summary>
        /// Gets the number of headers.
        /// </summary>
        public int Count => _headers.Count;

        /// <summary>
        /// Determines whether the specified header name exists.
        /// </summary>
        /// <param name="key">The header name to locate.</param>
        /// <returns>True if the header name is found in the dictionary, otherwise false.</returns>
        public bool ContainsKey(string key) => _headers.ContainsKey(key);

        /// <summary>
        /// Returns an enumerator that iterates through each header name/value.
        /// </summary>
        /// <returns>An enumerator that iterates through each header name/value.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _headers.GetEnumerator();

        /// <summary>
        /// Gets the value that is associated with the specified header name.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified header name,
        /// if the header name is found; otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// True if the dictionary contains a header with the specified name; otherwise false.
        /// </returns>
        public bool TryGetValue(string key, out object value) => _headers.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _headers.GetEnumerator();
    }
}
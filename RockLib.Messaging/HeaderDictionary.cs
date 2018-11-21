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
        /// <typeparamref name="T"/> type.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified header name,
        /// if the header name is found and its value can be converted to type <typeparamref name="T"/>;
        /// otherwise, the default value of type <typeparamref name="T"/>. This parameter is passed
        /// uninitialized.
        /// </param>
        /// <returns>
        /// True if the dictionary contains a header with the specified name and its value
        /// can be converted to type <typeparamref name="T"/>; otherwise false.
        /// </returns>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (_headers.TryGetValue(key, out object objectValue))
            {
                if (objectValue == null)
                {
                    value = default(T);
                    return false;
                }

                if (objectValue is T)
                {
                    value = (T)objectValue;
                    return true;
                }

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(objectValue.GetType()))
                {
                    try
                    {
                        value = (T)converter.ConvertFrom(objectValue);
                        return true;
                    }
                    catch
                    {
                    }
                }

                converter = TypeDescriptor.GetConverter(objectValue);
                if (converter.CanConvertTo(typeof(T)))
                {
                    try
                    {
                        value = (T)converter.ConvertTo(objectValue, typeof(T));
                        return true;
                    }
                    catch
                    {
                    }
                }
            }

            value = default(T);
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

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_headers).GetEnumerator();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
        public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
        {
            if (_headers.TryGetValue(key, out var objectValue))
            {
                switch (objectValue)
                {
                    case null:
                        value = default;
                        return false;
                    case T variable:
                        value = variable;
                        return true;
                }

                if (typeof(T) == typeof(DateTime) && objectValue is string stringValue)
                {
                    if (DateTime.TryParse(stringValue, null, DateTimeStyles.RoundtripKind, out var dateTimeValue))
                    {
                        value = (T)(object)dateTimeValue;
                        return true;
                    }
                }

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(objectValue.GetType()))
                {
                    value = (T)converter.ConvertFrom(objectValue)!;
                    return true;
                }

                converter = TypeDescriptor.GetConverter(objectValue);
                if (converter.CanConvertTo(typeof(T)))
                {
                    value = (T)converter.ConvertTo(objectValue, typeof(T))!;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value that is associated with the specified header name as a
        /// <typeparamref name="T"/> type.
        /// </summary>
        /// <param name="key">The name of the header.</param>
        /// <returns>
        /// The value associated with the specified header name if the header name is found and its
        /// value can be converted to type <typeparamref name="T"/>.
        /// </returns>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <exception cref="KeyNotFoundException">
        /// If the specified key cannot be found, or if its value is null.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// If the specified key can be found, but its value cannot be converted to type
        /// <typeparamref name="T"/>.
        /// </exception>
        public T GetValue<T>(string key)
        {
            if (!_headers.TryGetValue(key, out var objectValue))
            {
                throw new KeyNotFoundException($"The specified header, '{key}', was not found.");
            }

            switch (objectValue)
            {
                case null:
                    throw new KeyNotFoundException($"The specified header, '{key}', has a null value.");
                case T variable:
                    return variable;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(objectValue.GetType()))
            {
                return (T)converter.ConvertFrom(objectValue)!;
            }

            converter = TypeDescriptor.GetConverter(objectValue);
            if (converter.CanConvertTo(typeof(T)))
            {
                return (T)converter.ConvertTo(objectValue, typeof(T))!;
            }

            throw new InvalidCastException($"The specified header, '{key}', has a value, {objectValue} (with type {objectValue.GetType().FullName}), that cannot be converted to target type {typeof(T).FullName}.");
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
#if NET48
        public bool TryGetValue(string key, out object value) => _headers.TryGetValue(key, out value);
#else
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _headers.TryGetValue(key, out value);
#endif

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_headers).GetEnumerator();
    }
}
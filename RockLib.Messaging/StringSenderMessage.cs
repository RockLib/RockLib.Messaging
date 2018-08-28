using System;
using System.Collections.Generic;
using System.Text;

namespace RockLib.Messaging
{
    /// <summary>
    /// An implementation of <see cref="ISenderMessage"/> for use when a string is the
    /// payload of the message.
    /// </summary>
    public class StringSenderMessage : ISenderMessage
    {
        private readonly IDictionary<string, string> _headers = new Dictionary<string, string>();
        private readonly string _stringValue;
        private readonly Lazy<byte[]> _binaryValue;
        private readonly MessageFormat _messageFormat;
        private readonly byte? _priority;
        private readonly bool? _compressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringSenderMessage"/> class 
        /// using <see cref="Messaging.MessageFormat.Text"/> for its format. The value of
        /// the <see cref="BinaryValue"/> property will be the result of a call to the
        /// <see cref="Encoding.GetBytes(string)"/> method of <paramref name="encoding"/>.
        /// If <paramref name="encoding"/> is null, <see cref="Encoding.UTF8"/> is used.
        /// </summary>
        /// <param name="stringValue">The string value of the message.</param>
        /// <param name="encoding">
        /// The encoding to use when converting the string value to a byte array.
        /// </param>
        /// <param name="priority">The priority of the message.</param>
        /// <param name="compressed">
        /// Whether the message should be compressed. If null, compression is determined by
        /// the sender's configuration.
        /// </param>
        public StringSenderMessage(string stringValue, Encoding encoding = null, byte? priority = null, bool? compressed = null)
            : this(stringValue, MessageFormat.Text, encoding, priority, compressed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringSenderMessage"/> class.
        /// If the value of <paramref name="messageFormat"/> is <see cref="Messaging.MessageFormat.Binary"/>,
        /// then the value of the <see cref="BinaryValue"/> property will be the result of
        /// a call to <see cref="Convert.FromBase64String"/>. Otherwise, the value of the
        /// <see cref="BinaryValue"/> property will be the result of a call to the
        /// <see cref="Encoding.GetBytes(string)"/> method of <paramref name="encoding"/>.
        /// If <paramref name="encoding"/> is null, <see cref="Encoding.UTF8"/> is used.
        /// </summary>
        /// <param name="stringValue">The string value of the message.</param>
        /// <param name="messageFormat">The message's format.</param>
        /// <param name="encoding">
        /// The encoding to use when converting the string value to a byte array if
        /// <paramref name="messageFormat"/> is not <see cref="Messaging.MessageFormat.Binary"/>.
        /// </param>
        /// <param name="priority">The priority of the message.</param>
        /// <param name="compressed">
        /// Whether the message should be compressed. If null, compression is determined by
        /// the sender's configuration.
        /// </param>
        public StringSenderMessage(string stringValue, MessageFormat messageFormat, Encoding encoding = null, byte? priority = null, bool? compressed = null)
        {
            _stringValue = stringValue;
            _binaryValue =
                new Lazy<byte[]>(
                    () =>
                    stringValue == null
                        ? null
                        : messageFormat == MessageFormat.Binary
                            ? Convert.FromBase64String(stringValue)
                            : (encoding ?? Encoding.UTF8).GetBytes(stringValue));
            _messageFormat = messageFormat;
            _priority = priority;
            _compressed = compressed;
        }

        /// <summary>
        /// Gets the string value of the message.
        /// </summary>
        public string StringValue
        {
            get { return _stringValue; }
        }

        /// <summary>
        /// Gets the binary value of the message.
        /// </summary>
        public byte[] BinaryValue
        {
            get { return _binaryValue.Value; }
        }

        /// <summary>
        /// Gets the message format of the message.
        /// </summary>
        public MessageFormat MessageFormat
        {
            get { return _messageFormat; }
        }

        /// <summary>
        /// Gets the headers for this message.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get { return _headers; }
        }

        IEnumerable<KeyValuePair<string, string>> ISenderMessage.Headers
        {
            get { return Headers; }
        }

        /// <summary>
        /// Gets the priority of the message.
        /// </summary>
        public byte? Priority
        {
            get { return _priority; }
        }

        /// <summary>
        /// Gets a value indicating whether the message should be compressed when sending.
        /// If null, compression is determined by the sender's configuration.
        /// </summary>
        public bool? Compressed { get { return _compressed; } }
    }
}
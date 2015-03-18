using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.Messaging
{
    /// <summary>
    /// An implementation of <see cref="ISenderMessage"/> for use when a byte array is the
    /// payload of the message.
    /// </summary>
    public class BinarySenderMessage : ISenderMessage
    {
        private readonly IDictionary<string, string> _headers = new Dictionary<string, string>();
        private readonly byte[] _binaryValue;
        private readonly Lazy<string> _stringValue;
        private readonly MessageFormat _messageFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySenderMessage"/> class using
        /// <see cref="Messaging.MessageFormat.Binary"/> for its format. The value of
        /// <see cref="StringValue"/> will be the result of a base 64 encoding operation on
        /// <paramref name="binaryValue"/>.
        /// </summary>
        /// <param name="binaryValue">The binary value of the message.</param>
        public BinarySenderMessage(byte[] binaryValue)
            : this(binaryValue, MessageFormat.Binary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySenderMessage"/> class. If
        /// <paramref name="messageFormat"/> is <see cref="Messaging.MessageFormat.Binary"/>,
        /// the value of <see cref="StringValue"/> will be the result of a base 64 encoding
        /// operation on <paramref name="binaryValue"/>. Otherwise, <paramref name="encoding"/>
        /// (or <see cref="Encoding.UTF8"/> if null) will be used to obtain the value of
        /// <see cref="StringValue"/>.
        /// </summary>
        /// <param name="binaryValue">The binary value of the message.</param>
        /// <param name="messageFormat">The message's format.</param>
        /// <param name="encoding">
        /// The encoding to use when converting the binary value to a string if 
        /// <paramref name="messageFormat"/> is not <see cref="Messaging.MessageFormat.Binary"/>.
        /// </param>
        public BinarySenderMessage(byte[] binaryValue, MessageFormat messageFormat, Encoding encoding = null)
        {
            _binaryValue = binaryValue;
            _stringValue =
                new Lazy<string>(
                    () =>
                    binaryValue == null
                        ? null
                        : messageFormat == MessageFormat.Binary
                            ? Convert.ToBase64String(binaryValue)
                            : (encoding ?? Encoding.UTF8).GetString(binaryValue));
            _messageFormat = messageFormat;
        }

        /// <summary>
        /// Gets the string value of the message.
        /// </summary>
        public string StringValue
        {
            get { return _stringValue.Value; }
        }

        /// <summary>
        /// Gets the binary value of the message.
        /// </summary>
        public byte[] BinaryValue
        {
            get { return _binaryValue; }
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
    }
}
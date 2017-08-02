using System;
using System.Collections.Generic;
using System.Text;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
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
        private readonly byte? _priority;
        private readonly bool? _compressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySenderMessage"/> class using
        /// <see cref="Messaging.MessageFormat.Binary"/> for its format. The value of
        /// <see cref="StringValue"/> will be the result of a base 64 encoding operation on
        /// <paramref name="binaryValue"/>.
        /// </summary>
        /// <param name="binaryValue">The binary value of the message.</param>
        /// <param name="encoding">
        /// The encoding to use when converting the byte array value to a string.
        /// </param>
        /// <param name="priority">The priority of the message.</param>
        /// <param name="compressed">
        /// Whether the message should be compressed. If null, compression is determined by
        /// the sender's configuration.
        /// </param>
        public BinarySenderMessage(byte[] binaryValue, Encoding encoding = null, byte? priority = null, bool? compressed = null)
            : this(binaryValue, MessageFormat.Binary, encoding, priority, compressed)
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
        /// <paramref name="messageFormat"/> is not <see cref="Rock.Messaging.MessageFormat.Binary"/>.
        /// </param>
        /// <param name="priority">The priority of the message.</param>
        /// <param name="compressed">
        /// Whether the message should be compressed. If null, compression is determined by
        /// the sender's configuration.
        /// </param>
        public BinarySenderMessage(byte[] binaryValue, MessageFormat messageFormat, Encoding encoding = null, byte? priority = null, bool? compressed = null)
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
            _priority = priority;
            _compressed = compressed;
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
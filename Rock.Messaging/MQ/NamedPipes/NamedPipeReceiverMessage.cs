using System;
using System.Linq;
using System.Text;

#if ROCKLIB
using RockLib.Messaging.Internal;
#else
using Rock.Messaging.Internal;
#endif

#if ROCKLIB
namespace RockLib.Messaging.NamedPipes
#else
namespace Rock.Messaging.NamedPipes
#endif
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="NamedPipeQueueConsumer"/>
    /// class.
    /// </summary>
    public class NamedPipeReceiverMessage : IReceiverMessage
    {
        private readonly NamedPipeMessage _namedPipeMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeReceiverMessage"/> class.
        /// </summary>
        /// <param name="namedPipeMessage">The message that was sent.</param>
        internal NamedPipeReceiverMessage(NamedPipeMessage namedPipeMessage)
        {
            _namedPipeMessage = namedPipeMessage;
        }

        /// <summary>
        /// Gets the priority of the received message.
        /// </summary>
        public byte? Priority { get { return null; } }

        /// <summary>
        /// Gets the string value of the message. The <paramref name="encoding"/> parameter
        /// is ignored.
        /// </summary>
        /// <param name="encoding">Ignored.</param>
        /// <returns>
        /// The string value of the message.
        /// </returns>
        public string GetStringValue(Encoding encoding)
        {
            var stringValue = RawStringValue;

            if (_namedPipeMessage.Headers.ContainsKey(HeaderName.CompressedPayload)
                && _namedPipeMessage.Headers[HeaderName.CompressedPayload] == "true")
            {
                stringValue = MessageCompression.Decompress(stringValue);
            }

            return stringValue;
        }

        /// <summary>
        /// Gets the binary value of the message. The <paramref name="encoding"/> parameter
        /// is ignored.
        /// </summary>
        /// <param name="encoding">Ignored.</param>
        /// <returns>
        /// The binary value of the message.
        /// </returns>
        public byte[] GetBinaryValue(Encoding encoding)
        {
            var stringValue = GetStringValue(encoding);

            return
                stringValue == null
                    ? null
                    : MessageFormat == MessageFormat.Binary || encoding == null
                        ? Convert.FromBase64String(stringValue)
                        : encoding.GetBytes(stringValue);
        }

        /// <summary>
        /// Gets a header value by key. The <paramref name="encoding"/> parameter
        /// is ignored.
        /// </summary>
        /// <param name="key">The key of the header to retrieve.</param>
        /// <param name="encoding">Ignored.</param>
        /// <returns>The string value of the header.</returns>
        public string GetHeaderValue(string key, Encoding encoding)
        {
            string headerValue;

            if (_namedPipeMessage.Headers.TryGetValue(key, out headerValue))
            {
                return headerValue;
            }

            return null;
        }

        /// <summary>
        /// Gets the names of the headers that are available for this message.
        /// </summary>
        /// <returns>An array containing the names of the headers for this message.</returns>
        public string[] GetHeaderNames()
        {
            return _namedPipeMessage.Headers.Keys.ToArray();
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Acknowledge()
        {
        }

        /// <summary>
        /// Returns an instance of <see cref="ISenderMessage"/> that is equivalent to this
        /// instance of <see cref="NamedPipeReceiverMessage"/>.
        /// </summary>
        public ISenderMessage ToSenderMessage()
        {
            // If the received message is compressed, then it will already have the compression
            // header, so it will pass it along to the sender message. But we don't want to
            // double-compress the payload, so pass false for the compressed constructor parameter.
            var senderMessage = new StringSenderMessage(RawStringValue, MessageFormat, compressed: false);

            foreach (var header in _namedPipeMessage.Headers)
            {
                senderMessage.Headers.Add(header.Key, header.Value);
            }

            return senderMessage;
        }

        private string RawStringValue
        {
            get { return _namedPipeMessage.StringValue; }
        }

        private MessageFormat MessageFormat
        {
            get
            {
                if (_namedPipeMessage.Headers.ContainsKey(HeaderName.MessageFormat))
                {
                    MessageFormat messageFormat;
                    if (Enum.TryParse(_namedPipeMessage.Headers[HeaderName.MessageFormat], out messageFormat))
                    {
                        return messageFormat;
                    }
                }

                return MessageFormat.Text;
            }
        }
    }
}
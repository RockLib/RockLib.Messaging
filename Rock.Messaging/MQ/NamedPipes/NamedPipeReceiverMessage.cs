using System;
using System.Linq;
using System.Text;
using Rock.Messaging.Internal;

namespace Rock.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="NamedPipeQueueConsumer"/>
    /// class.
    /// </summary>
    public class NamedPipeReceiverMessage : IReceiverMessage
    {
        private readonly SentMessage _sentMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeReceiverMessage"/> class.
        /// </summary>
        /// <param name="sentMessage">The message that was sent.</param>
        internal NamedPipeReceiverMessage(SentMessage sentMessage)
        {
            _sentMessage = sentMessage;
        }

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

            if (_sentMessage.Headers.ContainsKey(HeaderName.CompressedPayload)
                && _sentMessage.Headers[HeaderName.CompressedPayload] == "true")
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

            if (_sentMessage.Headers.TryGetValue(key, out headerValue))
            {
                return headerValue;
            }

            return null;
        }

        public string[] GetHeaderNames()
        {
            return _sentMessage.Headers.Keys.ToArray();
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Acknowledge()
        {
        }

        public ISenderMessage ToSenderMessage()
        {
            // If the received message is compressed, then it will already have the compression
            // header, so it will pass it along to the sender message. But we don't want to
            // double-compress the payload, so pass false for the compressed constructor parameter.
            var senderMessage = new StringSenderMessage(RawStringValue, MessageFormat, compressed: false);

            foreach (var header in _sentMessage.Headers)
            {
                senderMessage.Headers.Add(header.Key, header.Value);
            }

            return senderMessage;
        }

        private string RawStringValue
        {
            get { return _sentMessage.StringValue; }
        }

        private MessageFormat MessageFormat
        {
            get
            {
                if (_sentMessage.Headers.ContainsKey(HeaderName.MessageFormat))
                {
                    MessageFormat messageFormat;
                    if (Enum.TryParse(_sentMessage.Headers[HeaderName.MessageFormat], out messageFormat))
                    {
                        return messageFormat;
                    }
                }

                return MessageFormat.Text;
            }
        }
    }
}
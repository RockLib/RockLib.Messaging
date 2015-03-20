using System.Text;

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
            return _sentMessage.StringValue;
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
            return _sentMessage.BinaryValue;
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

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Acknowledge()
        {
        }
    }
}
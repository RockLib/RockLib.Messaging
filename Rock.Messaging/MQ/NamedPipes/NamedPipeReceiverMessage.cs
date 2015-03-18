using System.Text;

namespace Rock.Messaging.NamedPipes
{
    public class NamedPipeReceiverMessage : IReceiverMessage
    {
        private readonly SentMessage _sentMessage;

        public NamedPipeReceiverMessage(SentMessage sentMessage)
        {
            _sentMessage = sentMessage;
        }

        public string GetStringValue(Encoding encoding)
        {
            return _sentMessage.StringValue;
        }

        public byte[] GetBinaryValue(Encoding encoding)
        {
            return _sentMessage.BinaryValue;
        }

        public string GetHeaderValue(string name, Encoding encoding)
        {
            string headerValue;

            if (_sentMessage.Headers.TryGetValue(name, out headerValue))
            {
                return headerValue;
            }

            return null;
        }

        public void Acknowledge()
        {
            // Nothing to do!
        }
    }
}
using System.Collections.Generic;

namespace Rock.Messaging.NamedPipes
{
    internal class SentMessage : ISenderMessage
    {
        public string StringValue { get; set; }
        public byte[] BinaryValue { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public MessageFormat MessageFormat { get; set; }

        IEnumerable<KeyValuePair<string, string>> ISenderMessage.Headers { get { return Headers; } }
        byte? ISenderMessage.Priority { get { return null; } }
        bool? ISenderMessage.Compressed { get { return null; } }
    }
}
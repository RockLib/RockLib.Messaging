using System.Collections.Generic;

namespace Rock.Messaging.NamedPipes
{
    public class SentMessage
    {
        public string StringValue { get; set; }
        public byte[] BinaryValue { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}
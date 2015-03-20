using System.Collections.Generic;

namespace Rock.Messaging.NamedPipes
{
    internal class SentMessage
    {
        public string StringValue { get; set; }
        public byte[] BinaryValue { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}
using System.Collections.Generic;

namespace Rock.Messaging.NamedPipes
{
    internal class SentMessage
    {
        public string StringValue { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public MessageFormat MessageFormat { get; set; }
        public byte? Priority { get; set; }
    }
}
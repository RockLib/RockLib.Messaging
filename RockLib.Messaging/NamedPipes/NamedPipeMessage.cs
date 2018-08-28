using System.Collections.Generic;

namespace RockLib.Messaging.NamedPipes
{
    internal class NamedPipeMessage
    {
        public string StringValue { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public MessageFormat MessageFormat { get; set; }
        public byte? Priority { get; set; }
    }
}
using System.Collections.Generic;

#if ROCKLIB
namespace RockLib.Messaging.NamedPipes
#else
namespace Rock.Messaging.NamedPipes
#endif
{
    internal class NamedPipeMessage
    {
        public string StringValue { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public MessageFormat MessageFormat { get; set; }
        public byte? Priority { get; set; }
    }
}
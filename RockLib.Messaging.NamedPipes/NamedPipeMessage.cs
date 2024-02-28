using System.Collections.Generic;

namespace RockLib.Messaging.NamedPipes
{
    internal sealed class NamedPipeMessage
    {
        public string? StringValue { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}
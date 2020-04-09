using RockLib.Messaging;
using System;

namespace Example.Messaging.NamedPipes.DotNetCore20
{
    class CommandSendingService : SendingService
    {
        public CommandSendingService(ISender sender)
            : base(sender, $"Enter command to send. (Legal values are: {string.Join(", ", Enum.GetNames(typeof(Casing)))})")
        {
        }
    }
}

using RockLib.Messaging;
using System;

namespace Example.Common
{
    public class CommandSendingService : SendingService
    {
        public CommandSendingService(ISender sender)
            : base(sender)
        {
        }

        protected override string Prompt => $"Enter command to send. (Legal values are: {string.Join(", ", Enum.GetNames(typeof(Casing)))})";
    }
}

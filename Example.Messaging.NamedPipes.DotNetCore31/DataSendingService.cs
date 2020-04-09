using RockLib.Messaging;

namespace Example.Messaging.NamedPipes.DotNetCore31
{
    class DataSendingService : SendingService
    {
        public DataSendingService(ISender sender)
            : base(sender)
        {
        }

        protected override string Prompt => "Enter data to send.";
    }
}

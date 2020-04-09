using RockLib.Messaging;

namespace Example.Messaging.NamedPipes.DotNetCore20
{
    class DataSendingService : SendingService
    {
        public DataSendingService(ISender sender)
            : base(sender, "Enter data to send.")
        {
        }
    }
}

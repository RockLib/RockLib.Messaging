using RockLib.Messaging;

namespace Example.Common
{
    public class DataSendingService : SendingService
    {
        public DataSendingService(ISender sender)
            : base(sender)
        {
        }

        protected override string Prompt => "Enter data to send.";
    }
}

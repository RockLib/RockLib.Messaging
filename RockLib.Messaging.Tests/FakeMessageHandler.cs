using System.Collections.Generic;

namespace RockLib.Messaging.Tests
{
    public class FakeMessageHandler : IMessageHandler
    {
        public List<(IReceiver Receiver, IReceiverMessage Message)> ReceivedMessages { get; } = new List<(IReceiver, IReceiverMessage)>();

        public void OnMessageReceived(IReceiver receiver, IReceiverMessage message)
        {
            ReceivedMessages.Add((receiver, message));
        }
    }
}

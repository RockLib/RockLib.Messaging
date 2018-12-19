using System.Collections.Generic;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    public class FakeMessageHandler : IMessageHandler
    {
        public List<(IReceiver Receiver, IReceiverMessage Message)> ReceivedMessages { get; } = new List<(IReceiver, IReceiverMessage)>();

        public Task OnMessageReceivedAsync(IReceiver receiver, IReceiverMessage message)
        {
            ReceivedMessages.Add((receiver, message));
            return Task.FromResult(0);
        }
    }
}

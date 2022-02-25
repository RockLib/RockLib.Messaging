using System.Collections.Generic;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    public class FakeMessageHandler : IMessageHandler
    {
#pragma warning disable CA1002 // Do not expose generic lists
        public List<(IReceiver Receiver, IReceiverMessage Message)> ReceivedMessages { get; } = new List<(IReceiver, IReceiverMessage)>();
#pragma warning restore CA1002 // Do not expose generic lists

        public Task OnMessageReceivedAsync(IReceiver receiver, IReceiverMessage message)
        {
            ReceivedMessages.Add((receiver, message));
            return Task.FromResult(0);
        }
    }
}

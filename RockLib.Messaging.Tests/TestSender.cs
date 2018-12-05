using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    public class TestSender : ISender
    {
        public List<SenderMessage> SentMessages { get; } = new List<SenderMessage>();

        public string Name => "TestSender";

        public void Dispose()
        {
        }

        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            SentMessages.Add(message);
            return Task.CompletedTask;
        }
    }
}

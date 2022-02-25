using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    public sealed class FakeSender : ISender
    {
#pragma warning disable CA1002 // Do not expose generic lists
        public List<SenderMessage> SentMessages { get; } = new List<SenderMessage>();
#pragma warning restore CA1002 // Do not expose generic lists

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
        public string PipeName { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

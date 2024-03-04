using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.NamedPipes.Tests
{
    public static class NamedPipesTests
    {
        [Fact]
        public static async Task NamedPipeMessagesAreSentAndReceived()
        {
            using var waitHandle = new AutoResetEvent(false);

            using var receiver = new NamedPipeReceiver("foo", "test-pipe");
            var payload = string.Empty;
            var headerValue = string.Empty;

            receiver.Start(async m =>
            {
                payload = m.StringPayload;
                headerValue = m.Headers.GetValue<string>("bar");
                await m.AcknowledgeAsync();
                waitHandle.Set();
            });

            using (var sender = new NamedPipeSender("foo", "test-pipe"))
            {
                await sender.SendAsync(new SenderMessage("Hello, world!") { Headers = { { "bar", "abc" } } });
                waitHandle.WaitOne();
            }

            Assert.Equal("Hello, world!", payload);
            Assert.Equal("abc", headerValue);
        }
    }
}

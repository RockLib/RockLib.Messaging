using System.Threading;
using Xunit;

namespace RockLib.Messaging.NamedPipes.Tests
{
    public class NamedPipesTests
    {
        [Fact]
        public void NamedPipeMessagesAreSentAndReceived()
        {
            var waitHandle = new AutoResetEvent(false);

            using (var receiver = new NamedPipeReceiver("foo", "test-pipe"))
            {
                string payload = null;
                string headerValue = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    headerValue = m.Headers.GetValue<string>("bar");
                    m.Acknowledge();
                    waitHandle.Set();
                });

                using (var sender = new NamedPipeSender("foo", "test-pipe"))
                {
                    sender.Send(new SenderMessage("Hello, world!") { Headers = { { "bar", "abc" } } });
                }

                waitHandle.WaitOne();

                Assert.Equal("Hello, world!", payload);
                Assert.Equal("abc", headerValue);
            }
        }
    }
}

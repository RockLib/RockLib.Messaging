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

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Acknowledge();
                    waitHandle.Set();
                });

                using (var sender = new NamedPipeSender("foo", "test-pipe"))
                {
                    sender.Send("Hello, world!");
                }

                waitHandle.WaitOne();

                Assert.Equal("Hello, world!", payload);
            }
        }
    }
}

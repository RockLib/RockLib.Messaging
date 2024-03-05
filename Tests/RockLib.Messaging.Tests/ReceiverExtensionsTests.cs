using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ReceiverExtensionsTests
    {
        [Fact]
        public async Task TheCallbackPassedToStart1IsInvokedWhenAMessageIsReceived()
        {
            using var receiver = new FakeReceiver();

            var received = false;

            receiver.Start(async m =>
            {
                received = true;
                await m.AcknowledgeAsync();
            });

            using var message = new FakeReceiverMessage("Hello, world!");
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            Assert.True(received);
        }

        [Fact]
        public async Task TheCallbackPassedToStart2IsInvokedWhenAMessageIsReceived()
        {
            using var receiver = new FakeReceiver();

            var received = false;

#pragma warning disable CS0618 // Type or member is obsolete
            receiver.Start(m =>
            {
                received = true;
                m.Acknowledge();
            });
#pragma warning restore CS0618 // Type or member is obsolete

            using var message = new FakeReceiverMessage("Hello, world!");
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            Assert.True(received);
        }

        [Fact]
        public async Task TheCallbackPassedToStart3IsInvokedWhenAMessageIsReceived()
        {
            using var receiver = new FakeReceiver();

            var received = false;

            receiver.Start(async (r, m) =>
            {
                received = true;
                await m.AcknowledgeAsync();
            });

            using var message = new FakeReceiverMessage("Hello, world!");
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            Assert.True(received);
        }

        [Fact]
        public async Task TheCallbackPassedToStart4IsInvokedWhenAMessageIsReceived()
        {
            using var receiver = new FakeReceiver();

            var received = false;

#pragma warning disable CS0618 // Type or member is obsolete
            receiver.Start((r, m) =>
            {
                received = true;
                m.Acknowledge();
            });
#pragma warning restore CS0618 // Type or member is obsolete

            using var message = new FakeReceiverMessage("Hello, world!");
            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, message);

            Assert.True(received);
        }
    }
}

using NUnit.Framework;
using RockLib.Messaging.Testing;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class ReceiverExtensionsTests
    {
        [Test]
        public async Task TheCallbackPassedToStart1IsInvokedWhenAMessageIsReceived()
        {
            var receiver = new FakeReceiver();

            var received = false;

            receiver.Start(async m =>
            {
                received = true;
                await m.AcknowledgeAsync();
            });

            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, new FakeReceiverMessage("Hello, world!"));

            Assert.True(received);
        }

        [Test]
        public async Task TheCallbackPassedToStart2IsInvokedWhenAMessageIsReceived()
        {
            var receiver = new FakeReceiver();

            var received = false;

#pragma warning disable CS0618 // Type or member is obsolete
            receiver.Start(m =>
            {
                received = true;
                m.Acknowledge();
            });
#pragma warning restore CS0618 // Type or member is obsolete

            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, new FakeReceiverMessage("Hello, world!"));

            Assert.True(received);
        }

        [Test]
        public async Task TheCallbackPassedToStart3IsInvokedWhenAMessageIsReceived()
        {
            var receiver = new FakeReceiver();

            var received = false;

            receiver.Start(async (r, m) =>
            {
                received = true;
                await m.AcknowledgeAsync();
            });

            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, new FakeReceiverMessage("Hello, world!"));

            Assert.True(received);
        }

        [Test]
        public async Task TheCallbackPassedToStart4IsInvokedWhenAMessageIsReceived()
        {
            var receiver = new FakeReceiver();

            var received = false;

#pragma warning disable CS0618 // Type or member is obsolete
            receiver.Start((r, m) =>
            {
                received = true;
                m.Acknowledge();
            });
#pragma warning restore CS0618 // Type or member is obsolete

            await receiver.MessageHandler.OnMessageReceivedAsync(receiver, new FakeReceiverMessage("Hello, world!"));

            Assert.True(received);
        }
    }
}

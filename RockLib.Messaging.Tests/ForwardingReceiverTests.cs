using FluentAssertions;
using NUnit.Framework;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class ForwardingReceiverTests
    {
        [Test]
        public void SettingMessageHandlerSetsTheMessageHandlerOfTheReceiverToAForwardingMessageHandler()
        {
            var receiver = new TestReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver);

            var messageHandler = new TestMessageHandler();

            forwardingReceiver.MessageHandler = messageHandler;

            forwardingReceiver.MessageHandler.Should().BeSameAs(messageHandler);

            receiver.MessageHandler.Should().NotBeSameAs(messageHandler);
            receiver.MessageHandler.Should().BeOfType<ForwardingMessageHandler>();
            ((ForwardingMessageHandler)receiver.MessageHandler).MessageHandler.Should().BeSameAs(messageHandler);
        }
    }
}

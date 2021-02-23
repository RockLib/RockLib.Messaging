using FluentAssertions;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ForwardingReceiverTests
    {
        [Fact]
        public void SettingMessageHandlerSetsTheMessageHandlerOfTheReceiverToAForwardingMessageHandler()
        {
            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver);

            var messageHandler = new FakeMessageHandler();

            forwardingReceiver.MessageHandler = messageHandler;

            forwardingReceiver.MessageHandler.Should().BeSameAs(messageHandler);

            receiver.MessageHandler.Should().NotBeSameAs(messageHandler);
            receiver.MessageHandler.Should().BeOfType<ForwardingMessageHandler>();
            ((ForwardingMessageHandler)receiver.MessageHandler).MessageHandler.Should().BeSameAs(messageHandler);
        }
    }
}

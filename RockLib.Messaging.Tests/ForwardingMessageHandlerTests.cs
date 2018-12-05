using FluentAssertions;
using NUnit.Framework;
using RockLib.Messaging.Testing;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class ForwardingMessageHandlerTests
    {
        [Test]
        public void OnMessageReceivedCallsInnerHandlerOnMessageReceivedWithForwardingReceiverMessage()
        {
            var receiver = new TestReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver);
            var messageHandler = new TestMessageHandler();

            var handler = new ForwardingMessageHandler(forwardingReceiver, messageHandler);

            var message = new TestReceiverMessage("Hello, world!");

            handler.OnMessageReceived(receiver, message);

            messageHandler.ReceivedMessages.Should().ContainSingle();
            messageHandler.ReceivedMessages[0].Receiver.Should().BeSameAs(forwardingReceiver);
            messageHandler.ReceivedMessages[0].Message.Should().BeOfType<ForwardingReceiverMessage>();
            ((ForwardingReceiverMessage)messageHandler.ReceivedMessages[0].Message).Message.Should().BeSameAs(message);
            ((ForwardingReceiverMessage)messageHandler.ReceivedMessages[0].Message).ForwardingReceiver.Should().BeSameAs(forwardingReceiver);
        }
    }
}

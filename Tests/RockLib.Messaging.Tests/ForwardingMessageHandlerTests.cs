using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ForwardingMessageHandlerTests
    {
        [Fact]
        public async Task OnMessageReceivedCallsInnerHandlerOnMessageReceivedWithForwardingReceiverMessage()
        {
            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver);
            var messageHandler = new FakeMessageHandler();

            var handler = new ForwardingMessageHandler(forwardingReceiver, messageHandler);

            using var message = new FakeReceiverMessage("Hello, world!");

            await handler.OnMessageReceivedAsync(receiver, message).ConfigureAwait(false);

            messageHandler.ReceivedMessages.Should().ContainSingle();
            messageHandler.ReceivedMessages[0].Receiver.Should().BeSameAs(forwardingReceiver);
            messageHandler.ReceivedMessages[0].Message.Should().BeOfType<ForwardingReceiverMessage>();
            ((ForwardingReceiverMessage)messageHandler.ReceivedMessages[0].Message).Message.Should().BeSameAs(message);
            ((ForwardingReceiverMessage)messageHandler.ReceivedMessages[0].Message).ForwardingReceiver.Should().BeSameAs(forwardingReceiver);
        }
    }
}

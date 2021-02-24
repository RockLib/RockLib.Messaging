using FluentAssertions;
using RockLib.Messaging.Testing;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ForwardingReceiverMessageTests
    {
        [Fact]
        public async Task AcknowledgeCallsInnerMessageAcknowledgeIfAcknowledgeForwarderIsNull()
        {
            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: null);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.AcknowledgeAsync();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.AcknowledgeAsync));
        }

        [Fact]
        public async Task AcknowledgeSendsMessageToAcknowledgeForwarderWhenAcknowledgeForwarderIsNotNull()
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: forwarder);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.AcknowledgeAsync();

            forwarder.SentMessages.Should().ContainSingle();
            forwarder.SentMessages[0].StringPayload.Should().Be("Hello, world!");
        }

        [Theory]
        [InlineData(ForwardingOutcome.Acknowledge)]
        [InlineData(ForwardingOutcome.Rollback)]
        [InlineData(ForwardingOutcome.Reject)]
        public async Task AcknowledgeHandlesInnerMessageAccordingToAcknowledgeOutcomeWhenAcknowledgeForwarderIsNotNull(ForwardingOutcome outcome)
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: forwarder, acknowledgeOutcome: outcome);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.AcknowledgeAsync();

            message.HandledBy.Should().Be($"{outcome}Async");
        }

        [Fact]
        public async Task RollbackCallsInnerMessageRollbackIfRollbackForwarderIsNull()
        {
            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: null);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RollbackAsync();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.RollbackAsync));
        }

        [Fact]
        public async Task RollbackSendsMessageToRollbackForwarderWhenRollbackForwarderIsNotNull()
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: forwarder);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RollbackAsync();

            forwarder.SentMessages.Should().ContainSingle();
            forwarder.SentMessages[0].StringPayload.Should().Be("Hello, world!");
        }

        [Theory]
        [InlineData(ForwardingOutcome.Acknowledge)]
        [InlineData(ForwardingOutcome.Rollback)]
        [InlineData(ForwardingOutcome.Reject)]
        public async Task RollbackHandlesInnerMessageAccordingToRollbackOutcomeWhenRollbackForwarderIsNotNull(ForwardingOutcome outcome)
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: forwarder, rollbackOutcome: outcome);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RollbackAsync();

            message.HandledBy.Should().Be($"{outcome}Async");
        }

        [Fact]
        public async Task RejectCallsInnerMessageRejectWhenRejectForwarderIsNull()
        {
            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: null);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RejectAsync();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.RejectAsync));
        }

        [Fact]
        public async Task RejectSendsMessageToRejectForwarderWhenRejectForwarderIsNotNull()
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: forwarder);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RejectAsync();

            forwarder.SentMessages.Should().ContainSingle();
            forwarder.SentMessages[0].StringPayload.Should().Be("Hello, world!");
        }

        [Theory]
        [InlineData(ForwardingOutcome.Acknowledge)]
        [InlineData(ForwardingOutcome.Rollback)]
        [InlineData(ForwardingOutcome.Reject)]
        public async Task RejectHandlesInnerMessageAccordingToRejectOutcomeWhenRejectForwarderIsNotNull(ForwardingOutcome outcome)
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: forwarder, rejectOutcome: outcome);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RejectAsync();

            message.HandledBy.Should().Be($"{outcome}Async");
        }
    }
}

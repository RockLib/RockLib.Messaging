using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ForwardingReceiverMessageTests
    {
        [Fact]
        public async Task AcknowledgeCallsInnerMessageAcknowledgeIfAcknowledgeForwarderIsNull()
        {
            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: null);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.AcknowledgeAsync();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.AcknowledgeAsync));
        }

        [Fact]
        public async Task AcknowledgeSendsMessageToAcknowledgeForwarderWhenAcknowledgeForwarderIsNotNull()
        {
            using var forwarder = new FakeSender();

            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: forwarder);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

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
            using var forwarder = new FakeSender();

            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: forwarder, acknowledgeOutcome: outcome);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.AcknowledgeAsync();

            message.HandledBy.Should().Be($"{outcome}Async");
        }

        [Fact]
        public async Task RollbackCallsInnerMessageRollbackIfRollbackForwarderIsNull()
        {
            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: null);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RollbackAsync();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.RollbackAsync));
        }

        [Fact]
        public async Task RollbackSendsMessageToRollbackForwarderWhenRollbackForwarderIsNotNull()
        {
            using var forwarder = new FakeSender();

            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: forwarder);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

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
            using var forwarder = new FakeSender();

            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: forwarder, rollbackOutcome: outcome);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RollbackAsync();

            message.HandledBy.Should().Be($"{outcome}Async");
        }

        [Fact]
        public async Task RejectCallsInnerMessageRejectWhenRejectForwarderIsNull()
        {
            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: null);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RejectAsync();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.RejectAsync));
        }

        [Fact]
        public async Task RejectSendsMessageToRejectForwarderWhenRejectForwarderIsNotNull()
        {
            using var forwarder = new FakeSender();

            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: forwarder);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

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
            using var forwarder = new FakeSender();

            using var receiver = new FakeReceiver();
            using var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: forwarder, rejectOutcome: outcome);
            using var message = new FakeReceiverMessage("Hello, world!");

            using var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            await forwardingMessage.RejectAsync();

            message.HandledBy.Should().Be($"{outcome}Async");
        }
    }
}

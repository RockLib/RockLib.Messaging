using FluentAssertions;
using NUnit.Framework;
using RockLib.Messaging.Testing;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class ForwardingReceiverMessageTests
    {
        [Test]
        public void AcknowledgeCallsInnerMessageAcknowledgeIfAcknowledgeForwarderIsNull()
        {
            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: null);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Acknowledge();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.AcknowledgeAsync));
        }

        [Test]
        public void AcknowledgeSendsMessageToAcknowledgeForwarderWhenAcknowledgeForwarderIsNotNull()
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: forwarder);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Acknowledge();

            forwarder.SentMessages.Should().ContainSingle();
            forwarder.SentMessages[0].StringPayload.Should().Be("Hello, world!");
        }

        [TestCase(ForwardingOutcome.Acknowledge)]
        [TestCase(ForwardingOutcome.Rollback)]
        [TestCase(ForwardingOutcome.Reject)]
        public void AcknowledgeHandlesInnerMessageAccordingToAcknowledgeOutcomeWhenAcknowledgeForwarderIsNotNull(ForwardingOutcome outcome)
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, acknowledgeForwarder: forwarder, acknowledgeOutcome: outcome);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Acknowledge();

            message.HandledBy.Should().Be($"{outcome}Async");
        }

        [Test]
        public void RollbackCallsInnerMessageRollbackIfRollbackForwarderIsNull()
        {
            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: null);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Rollback();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.RollbackAsync));
        }

        [Test]
        public void RollbackSendsMessageToRollbackForwarderWhenRollbackForwarderIsNotNull()
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: forwarder);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Rollback();

            forwarder.SentMessages.Should().ContainSingle();
            forwarder.SentMessages[0].StringPayload.Should().Be("Hello, world!");
        }

        [TestCase(ForwardingOutcome.Acknowledge)]
        [TestCase(ForwardingOutcome.Rollback)]
        [TestCase(ForwardingOutcome.Reject)]
        public void RollbackHandlesInnerMessageAccordingToRollbackOutcomeWhenRollbackForwarderIsNotNull(ForwardingOutcome outcome)
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rollbackForwarder: forwarder, rollbackOutcome: outcome);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Rollback();

            message.HandledBy.Should().Be($"{outcome}Async");
        }

        [Test]
        public void RejectCallsInnerMessageRejectWhenRejectForwarderIsNull()
        {
            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: null);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Reject();

            message.HandledBy.Should().Be(nameof(IReceiverMessage.RejectAsync));
        }

        [Test]
        public void RejectSendsMessageToRejectForwarderWhenRejectForwarderIsNotNull()
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: forwarder);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Reject();

            forwarder.SentMessages.Should().ContainSingle();
            forwarder.SentMessages[0].StringPayload.Should().Be("Hello, world!");
        }

        [TestCase(ForwardingOutcome.Acknowledge)]
        [TestCase(ForwardingOutcome.Rollback)]
        [TestCase(ForwardingOutcome.Reject)]
        public void RejectHandlesInnerMessageAccordingToRejectOutcomeWhenRejectForwarderIsNotNull(ForwardingOutcome outcome)
        {
            var forwarder = new FakeSender();

            var receiver = new FakeReceiver();
            var forwardingReceiver = new ForwardingReceiver("foo", receiver, rejectForwarder: forwarder, rejectOutcome: outcome);
            var message = new FakeReceiverMessage("Hello, world!");

            var forwardingMessage = new ForwardingReceiverMessage(forwardingReceiver, message);

            forwardingMessage.Reject();

            message.HandledBy.Should().Be($"{outcome}Async");
        }
    }
}

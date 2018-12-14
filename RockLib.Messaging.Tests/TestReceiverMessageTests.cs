using FluentAssertions;
using NUnit.Framework;
using RockLib.Messaging.Testing;
using System;
using System.Text;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class TestReceiverMessageTests
    {
        [Test]
        public void StringPayloadIsSetFromStringPayload()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.StringPayload.Should().BeSameAs("Hello, world!");
        }

        [Test]
        public void BinaryPayloadIsSetFromUTF8EncodedStringPayload()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("Hello, world!"), x => x.WithStrictOrdering());
        }

        [Test]
        public void BinaryPayloadIsSetFromBinaryPayload()
        {
            var payload = new byte[] { 1, 2, 3, 4, 5 };

            var message = new FakeReceiverMessage(payload);

            message.BinaryPayload.Should().BeSameAs(payload);
        }

        [Test]
        public void StringPayloadIsSetFromBase64EncodedBinaryPayload()
        {
            var payload = new byte[] { 1, 2, 3, 4, 5 };

            var message = new FakeReceiverMessage(payload);

            message.StringPayload.Should().Be(Convert.ToBase64String(payload));
        }

        [Test]
        public void PublicHeaderPropertyIsNotTheSameAsExplicitInterfaceHeaderProperty()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            ((object)message.Headers).Should().NotBeSameAs(((IReceiverMessage)message).Headers);
        }

        [Test]
        public void PublicHeaderPropertyHasSameContentsAsExplicitInterfaceHeaderProperty()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Headers.Add("foo", "abc");
            message.Headers.Add("bar", 123);

            HeaderDictionary interfaceHeaders = ((IReceiverMessage)message).Headers;

            interfaceHeaders.ContainsKey("foo");
            interfaceHeaders["foo"].Should().Be("abc");

            interfaceHeaders.ContainsKey("bar");
            interfaceHeaders["bar"].Should().Be(123);
        }

        [Test]
        public void HandledIsFalseBeforeMessageIsHandled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Handled.Should().BeFalse();
        }

        [Test]
        public void HandledByIsNullBeforeMessageIsHandled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.HandledBy.Should().BeNull();
        }

        [Test]
        public void HandledIsTrueAfterAcknowledgeIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Acknowledge();

            message.Handled.Should().BeTrue();
        }

        [Test]
        public void HandledByIsAcknowledgeAfterAcknowledgeIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Acknowledge();

            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        [Test]
        public void HandledIsTrueAfterRollbackIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Rollback();

            message.Handled.Should().BeTrue();
        }

        [Test]
        public void HandledByIsRollbackAfterRollbackIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Rollback();

            message.HandledBy.Should().Be(nameof(message.RollbackAsync));
        }

        [Test]
        public void HandledIsTrueAfterRejectIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Reject();

            message.Handled.Should().BeTrue();
        }

        [Test]
        public void HandledByIsRejectAfterRejectIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Reject();

            message.HandledBy.Should().Be(nameof(message.RejectAsync));
        }

        [Test]
        public void AcknowledgeThrowsIfHandledIsTrue()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Acknowledge();

            Action act = () => message.Acknowledge();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"Cannot {nameof(message.AcknowledgeAsync)} message: the message has already been handled by {nameof(message.AcknowledgeAsync)}.");
        }

        [Test]
        public void RollbackThrowsIfHandledIsTrue()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Acknowledge();

            Action act = () => message.Rollback();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"Cannot {nameof(message.RollbackAsync)} message: the message has already been handled by {nameof(message.AcknowledgeAsync)}.");
        }

        [Test]
        public void RejectThrowsIfHandledIsTrue()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Acknowledge();

            Action act = () => message.Reject();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"Cannot {nameof(message.RejectAsync)} message: the message has already been handled by {nameof(message.AcknowledgeAsync)}.");
        }
    }
}

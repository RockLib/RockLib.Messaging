using FluentAssertions;
using RockLib.Messaging.Testing;
using System;
using System.Text;
using Xunit;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    public class TestReceiverMessageTests
    {
        [Fact]
        public void StringPayloadIsSetFromStringPayload()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.StringPayload.Should().BeSameAs("Hello, world!");
        }

        [Fact]
        public void BinaryPayloadIsSetFromUTF8EncodedStringPayload()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("Hello, world!"), x => x.WithStrictOrdering());
        }

        [Fact]
        public void BinaryPayloadIsSetFromBinaryPayload()
        {
            var payload = new byte[] { 1, 2, 3, 4, 5 };

            var message = new FakeReceiverMessage(payload);

            message.BinaryPayload.Should().BeSameAs(payload);
        }

        [Fact]
        public void StringPayloadIsSetFromBase64EncodedBinaryPayload()
        {
            var payload = new byte[] { 1, 2, 3, 4, 5 };

            var message = new FakeReceiverMessage(payload);

            message.StringPayload.Should().Be(Convert.ToBase64String(payload));
        }

        [Fact]
        public void PublicHeaderPropertyIsNotTheSameAsExplicitInterfaceHeaderProperty()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            ((object)message.Headers).Should().NotBeSameAs(((IReceiverMessage)message).Headers);
        }

        [Fact]
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

        [Fact]
        public void HandledIsFalseBeforeMessageIsHandled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.Handled.Should().BeFalse();
        }

        [Fact]
        public void HandledByIsNullBeforeMessageIsHandled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            message.HandledBy.Should().BeNull();
        }

        [Fact]
        public async Task HandledIsTrueAfterAcknowledgeIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.AcknowledgeAsync();

            message.Handled.Should().BeTrue();
        }

        [Fact]
        public async Task HandledByIsAcknowledgeAfterAcknowledgeIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.AcknowledgeAsync();

            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        [Fact]
        public async Task HandledIsTrueAfterRollbackIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.RollbackAsync();

            message.Handled.Should().BeTrue();
        }

        [Fact]
        public async Task HandledByIsRollbackAfterRollbackIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.RollbackAsync();

            message.HandledBy.Should().Be(nameof(message.RollbackAsync));
        }

        [Fact]
        public async Task HandledIsTrueAfterRejectIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.RejectAsync();

            message.Handled.Should().BeTrue();
        }

        [Fact]
        public async Task HandledByIsRejectAfterRejectIsCalled()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.RejectAsync();

            message.HandledBy.Should().Be(nameof(message.RejectAsync));
        }

        [Fact]
        public async Task AcknowledgeThrowsIfHandledIsTrue()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.AcknowledgeAsync();

            Func<Task> act = () => message.AcknowledgeAsync();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"Cannot {nameof(message.AcknowledgeAsync)} message: the message has already been handled by {nameof(message.AcknowledgeAsync)}.");
        }

        [Fact]
        public async Task RollbackThrowsIfHandledIsTrue()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.AcknowledgeAsync();

            Func<Task> act = () => message.RollbackAsync();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"Cannot {nameof(message.RollbackAsync)} message: the message has already been handled by {nameof(message.AcknowledgeAsync)}.");
        }

        [Fact]
        public async Task RejectThrowsIfHandledIsTrue()
        {
            var message = new FakeReceiverMessage("Hello, world!");

            await message.AcknowledgeAsync();

            Func<Task> act = () => message.RejectAsync();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"Cannot {nameof(message.RejectAsync)} message: the message has already been handled by {nameof(message.AcknowledgeAsync)}.");
        }
    }
}

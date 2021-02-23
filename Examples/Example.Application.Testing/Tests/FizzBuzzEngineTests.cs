using FluentAssertions;
using Moq;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Example.Tests
{
    public class FizzBuzzEngineTests
    {
        [Fact(DisplayName = "Constructor sets Sender property from sender parameter")]
        public void ConstructorHappyPath()
        {
            var sender = new Mock<ISender>().Object;

            var engine = new FizzBuzzEngine(sender);

            engine.Sender.Should().BeSameAs(sender);
        }

        [Fact(DisplayName = "Constructor throws if sender parameter is null")]
        public void ConstructorSadPath()
        {
            Action act = () => new FizzBuzzEngine(null);

            act.Should().ThrowExactly<ArgumentNullException>()
                .WithMessage("*sender*");
        }

        [Fact(DisplayName = "SendFizzBuzzMessage sends 'fizz-buzz' for values divisible by both 3 and 5")]
        public async Task SendFizzBuzzMessageHappyPath1()
        {
            var mockSender = new Mock<ISender>();

            var engine = new FizzBuzzEngine(mockSender.Object);

            await engine.SendFizzBuzzMessage(30);

            mockSender.Verify(m => m.SendAsync(
                    It.Is<SenderMessage>(message => message.StringPayload == "fizz-buzz"),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact(DisplayName = "SendFizzBuzzMessage sends 'fizz' for values divisible by 3")]
        public async Task SendFizzBuzzMessageHappyPath2()
        {
            var mockSender = new Mock<ISender>();

            var engine = new FizzBuzzEngine(mockSender.Object);

            await engine.SendFizzBuzzMessage(9);

            mockSender.Verify(m => m.SendAsync(
                    It.Is<SenderMessage>(message => message.StringPayload == "fizz"),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact(DisplayName = "SendFizzBuzzMessage sends 'buzz' for values divisible by 5")]
        public async Task SendFizzBuzzMessageHappyPath3()
        {
            var mockSender = new Mock<ISender>();

            var engine = new FizzBuzzEngine(mockSender.Object);

            await engine.SendFizzBuzzMessage(20);

            mockSender.Verify(m => m.SendAsync(
                    It.Is<SenderMessage>(message => message.StringPayload == "buzz"),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact(DisplayName = "SendFizzBuzzMessage sends the value itself when not divisible by 3 or 5")]
        public async Task SendFizzBuzzMessageHappyPath4()
        {
            var mockSender = new Mock<ISender>();

            var engine = new FizzBuzzEngine(mockSender.Object);

            await engine.SendFizzBuzzMessage(14);

            mockSender.Verify(m => m.SendAsync(
                    It.Is<SenderMessage>(message => message.StringPayload == "14"),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact(DisplayName = "SendFizzBuzzMessage throws for values less than 1")]
        public async Task SendFizzBuzzMessageSadPath()
        {
            var mockSender = new Mock<ISender>();

            var engine = new FizzBuzzEngine(mockSender.Object);

            Func<Task> act = () => engine.SendFizzBuzzMessage(0);

            await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>()
                .WithMessage("Must be greater than zero.*value*");

            mockSender.Verify(m => m.SendAsync(
                    It.IsAny<SenderMessage>(),
                    It.IsAny<CancellationToken>()),
                Times.Never());
        }
    }
}

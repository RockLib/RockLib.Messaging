using FluentAssertions;
using Moq;
using Moq.Protected;
using RockLib.Dynamic;
using RockLib.Messaging;
using System;
using System.Threading;
using Xunit;
namespace Example.Common.Tests
{
    public class SingleMessageServiceTests
    {
        [Fact(DisplayName = "Constructor sets properties")]
        public void ConstructorHappyPath()
        {
            // Arrange
            var sender = new Mock<ISender>().Object;
            var receiver = new Mock<IReceiver>().Object;

            // Act
            var service = new SingleMessageService(sender, receiver);

            // Assert
            service.Sender.Should().BeSameAs(sender);
            service.Receiver.Should().BeSameAs(receiver);
        }

        [Fact(DisplayName = "Constructor throws when sender parameter is null")]
        public void ConstructorSadPath1()
        {
            // Arrange
            var receiver = new Mock<IReceiver>().Object;

            Action act = () => new SingleMessageService(null, receiver);

            // Act/Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*sender*");
        }

        [Fact(DisplayName = "Constructor throws when receiver parameter is null")]
        public void ConstructorSadPath2()
        {
            // Arrange
            var sender = new Mock<ISender>().Object;

            Action act = () => new SingleMessageService(sender, null);

            // Act/Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact(DisplayName = "WaitAndSendMessage waits then sends a message with the current date/time")]
        public void WaitAndSendMessage()
        {
            // Arrange
            var mockSender = new Mock<ISender>();
            var receiver = new Mock<IReceiver>().Object;

            var mockService = new Mock<SingleMessageService>(mockSender.Object, receiver);

            var now = DateTime.Now;

            mockService.Protected().As<IProtected>()
                .Setup(m => m.Now).Returns(now);

            var service = mockService.Object.Unlock();

            // Act
            service.WaitAndSendMessage();

            // Assert
            mockService.Protected().As<IProtected>()
                .Verify(m => m.Wait(), Times.Once());

            mockSender.Verify(m => m.SendAsync(It.Is<SenderMessage>(m => m.StringPayload == $"[{now:G}] Example message"), It.IsAny<CancellationToken>()),
                Times.Once());
        }

        private interface IProtected
        {
            void Wait();
            DateTime Now { get; }
        }
    }
}

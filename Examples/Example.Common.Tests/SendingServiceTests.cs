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
    public class SendingServiceTests
    {
        [Fact(DisplayName = "Constructor sets properties")]
        public void ConstructorHappyPath()
        {
            // Arrange
            var sender = new Mock<ISender>().Object;

            // Act
            var sendingService = new ConcreteSendingService(sender);

            // Assert
            sendingService.Sender.Should().BeSameAs(sender);
        }

        [Fact(DisplayName = "Constructor throws when sender parameter is null")]
        public void ConstructorSadPath()
        {
            // Arrange
            Action act = () => new ConcreteSendingService(null);

            // Act/Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*sender*");
        }

        [Fact(DisplayName = "ReadAndSendMessage reads a line and sends it")]
        public void ReadAndSendMessage()
        {
            // Arrange
            var mockSender = new Mock<ISender>();

            var mockSendingService = new Mock<SendingService>(mockSender.Object);

            mockSendingService.Protected().As<IProtected>()
                .Setup(m => m.ReadLine()).Returns("test-message");

            var sendingService = mockSendingService.Object.Unlock();

            // Act
            sendingService.ReadAndSendMessage();

            // Assert
            mockSendingService.Protected().As<IProtected>()
                .Verify(m => m.ReadLine(), Times.Once());

            mockSender.Verify(m => m.SendAsync(It.Is<SenderMessage>(m => m.StringPayload == "test-message"), It.IsAny<CancellationToken>()),
                Times.Once());
        }

        private class ConcreteSendingService : SendingService
        {
            public ConcreteSendingService(ISender sender)
                : base(sender)
            {
            }

            protected override string Prompt => null;
        }

        private interface IProtected
        {
            string ReadLine();
        }
    }
}

using FluentAssertions;
using Moq;
using Moq.Protected;
using RockLib.Dynamic;
using RockLib.Messaging;
using RockLib.Messaging.DependencyInjection;
using RockLib.Messaging.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Example.Common.Tests
{
    public class ReceivingServiceTests
    {
        [Fact(DisplayName = "Constructor sets properties using ReceiverLookup delegate")]
        public void ConstructorHappyPath()
        {
            // Arrange
            var dataReceiver = GetMockReceiver(ReceivingService.DataReceiverName).Object;
            var commandReceiver = GetMockReceiver(ReceivingService.CommandReceiverName).Object;

            var receiverLookup = GetReceiverLookup(dataReceiver, commandReceiver);

            // Act
            var receivingService = new ReceivingService(receiverLookup);

            // Assert
            receivingService.DataReceiver.Should().BeSameAs(dataReceiver);
            receivingService.CommandReceiver.Should().BeSameAs(commandReceiver);
            receivingService.Casing.Should().Be(Casing.Default);
        }

        [Fact(DisplayName = "Constructor throws when receiverLookup parameter is null")]
        public void ConstructorSadPath1()
        {
            // Arrange
            Action act = () => new ReceivingService(null);

            // Act/Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiverLookup*");
        }

        [Fact(DisplayName = "Constructor throws when receiverLookup returns null given '" + ReceivingService.DataReceiverName + "'")]
        public void ConstructorSadPath2()
        {
            // Arrange
            var commandReceiver = GetMockReceiver(ReceivingService.CommandReceiverName).Object;
            var receiverLookup = GetReceiverLookup(null, commandReceiver); ;

            Action act = () => new ReceivingService(receiverLookup);

            // Act/Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("*DataReceiver*receiverLookup*");
        }

        [Fact(DisplayName = "Constructor throws when receiverLookup returns null given '" + ReceivingService.CommandReceiverName + "'")]
        public void ConstructorSadPath3()
        {
            // Arrange
            var dataReceiver = GetMockReceiver(ReceivingService.DataReceiverName).Object;
            var receiverLookup = GetReceiverLookup(dataReceiver, null);

            Action act = () => new ReceivingService(receiverLookup);

            // Act/Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("*CommandReceiver*receiverLookup*");
        }

        [Fact(DisplayName = "DataReceivedAsync writes the formatted message payload")]
        public async Task DataReceivedAsyncHappyPath()
        {
            // Arrange
            var dataReceiver = GetMockReceiver(ReceivingService.DataReceiverName).Object;
            var commandReceiver = GetMockReceiver(ReceivingService.CommandReceiverName).Object;

            var mockReceivingService = new Mock<ReceivingService>(GetReceiverLookup(dataReceiver, commandReceiver));

            mockReceivingService.Protected().As<IProtected>()
                .Setup(m => m.WriteLine(It.IsAny<string>()))
                .Verifiable();

            mockReceivingService.Protected().As<IProtected>()
                .Setup(m => m.FormatMessage(It.IsAny<string>()))
                .Returns("formatted-message")
                .Verifiable();

            var receivingService = mockReceivingService.Object.Unlock();

            var message = new FakeReceiverMessage("test-message");

            // Act
            await receivingService.DataReceivedAsync(message);

            // Assert
            mockReceivingService.Protected().As<IProtected>()
                .Verify(m => m.WriteLine("formatted-message"), Times.Once());

            mockReceivingService.Protected().As<IProtected>()
                .Verify(m => m.FormatMessage("test-message"), Times.Once());

            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        [Theory(DisplayName = "FormatMessage applies casing according to the current Casing")]
        [InlineData(Casing.Default, "ABCxyz")]
        [InlineData(Casing.UPPER, "ABCXYZ")]
        [InlineData(Casing.lower, "abcxyz")]
        [InlineData(Casing.SpOnGeBoB, "AbCxYz")]
        public void FormatMessageHappyPath(Casing casing, string expectedFormattedMessage)
        {
            // Arrange
            var dataReceiver = GetMockReceiver(ReceivingService.DataReceiverName).Object;
            var commandReceiver = GetMockReceiver(ReceivingService.CommandReceiverName).Object;

            var receivingService = new ReceivingService(GetReceiverLookup(dataReceiver, commandReceiver)).Unlock();

            receivingService.Casing = casing;

            // Act
            string formattedMessage = receivingService.FormatMessage("ABCxyz");

            // Assert
            formattedMessage.Should().Be(expectedFormattedMessage);
        }

        [Theory(DisplayName = "CommandReceivedAsync sets Casing from message payload")]
        [InlineData("default", Casing.Default)]
        [InlineData("upper", Casing.UPPER)]
        [InlineData("lower", Casing.lower)]
        [InlineData("spongebob", Casing.SpOnGeBoB)]
        public async Task CommandReceivedAsyncHappyPath1(string stringPayload, Casing expectedCasing)
        {
            // Arrange
            var dataReceiver = GetMockReceiver(ReceivingService.DataReceiverName).Object;
            var commandReceiver = GetMockReceiver(ReceivingService.CommandReceiverName).Object;

            var receivingService = new ReceivingService(GetReceiverLookup(dataReceiver, commandReceiver));

            var message = new FakeReceiverMessage(stringPayload);

            // Act
            await receivingService.Unlock().CommandReceivedAsync(message);

            // Assert
            receivingService.Casing.Should().Be(expectedCasing);

            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        [Fact(DisplayName = "CommandReceivedAsync does not change Casing when message payload is invalid")]
        public async Task CommandReceivedAsyncHappyPath2()
        {
            // Arrange
            var dataReceiver = GetMockReceiver(ReceivingService.DataReceiverName).Object;
            var commandReceiver = GetMockReceiver(ReceivingService.CommandReceiverName).Object;

            var receivingService = new ReceivingService(GetReceiverLookup(dataReceiver, commandReceiver));

            var message = new FakeReceiverMessage("UnknownCasing");

            // Assume
            receivingService.Casing.Should().Be(Casing.Default);

            // Act
            await receivingService.Unlock().CommandReceivedAsync(message);

            // Assert
            receivingService.Casing.Should().Be(Casing.Default);

            message.HandledBy.Should().Be(nameof(message.AcknowledgeAsync));
        }

        private static ReceiverLookup GetReceiverLookup(IReceiver dataReceiver, IReceiver commandReceiver) =>
            name => name switch
            {
                ReceivingService.DataReceiverName => dataReceiver,
                ReceivingService.CommandReceiverName => commandReceiver,
                _ => null
            };

        private Mock<IReceiver> GetMockReceiver(string name)
        {
            var mockReceiver = new Mock<IReceiver>();
            mockReceiver.Setup(m => m.Name).Returns(name);
            return mockReceiver;
        }

        private interface IProtected
        {
            void WriteLine(string message);
            string FormatMessage(string payload);
        }
    }
}

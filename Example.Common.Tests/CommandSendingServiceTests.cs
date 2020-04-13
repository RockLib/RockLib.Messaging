using FluentAssertions;
using Moq;
using RockLib.Dynamic;
using RockLib.Messaging;
using System;
using Xunit;

namespace Example.Common.Tests
{
    public class CommandSendingServiceTests
    {
        [Fact(DisplayName = "Constructor sets properties")]
        public void ConstructorHappyPath()
        {
            // Arrange
            var sender = new Mock<ISender>().Object;

            // Act
            var sendingService = new CommandSendingService(sender);

            // Assert
            sendingService.Sender.Should().BeSameAs(sender);
        }

        [Fact(DisplayName = "Constructor throws when sender parameter is null")]
        public void ConstructorSadPath()
        {
            // Arrange
            Action act = () => new CommandSendingService(null);

            // Act/Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*sender*");
        }

        [Fact(DisplayName = "Prompt returns correct message")]
        public void Prompt()
        {
            // Arrange
            var sender = new Mock<ISender>().Object;

            var sendingService = new CommandSendingService(sender).Unlock();

            // Act
            string prompt = sendingService.Prompt;

            // Assert
            prompt.Should().Be("Enter command to send. (Legal values are: Default, UPPER, lower, SpOnGeBoB)");
        }
    }
}

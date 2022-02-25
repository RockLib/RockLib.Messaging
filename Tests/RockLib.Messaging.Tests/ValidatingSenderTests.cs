using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ValidatingSenderTests
    {
        [Fact]
        public void ConstructorHappyPath()
        {
            var mockSender = new Mock<ISender>();
            Action<SenderMessage> validate = message => { };

            using var validatingSender = new ValidatingSender("Foo", mockSender.Object, validate);

            validatingSender.Name.Should().Be("Foo");
            validatingSender.Sender.Should().BeSameAs(mockSender.Object);
            validatingSender.Validate.Should().BeSameAs(validate);
        }

        [Fact]
        public void ConstructorSadPath1()
        {
            ISender sender = new Mock<ISender>().Object;
            Action<SenderMessage> validate = message => { };

            Func<ValidatingSender> act = () => new ValidatingSender(null!, sender, validate);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*name*");
        }

        [Fact]
        public void ConstructorSadPath2()
        {
            Action<SenderMessage> validate = message => { };

            Func<ValidatingSender> act = () => new ValidatingSender("Foo", null!, validate);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*sender*");
        }

        [Fact]
        public void ConstructorSadPath3()
        {
            ISender sender = new Mock<ISender>().Object;

            Func<ValidatingSender> act = () => new ValidatingSender("Foo", sender, null!);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*validate*");
        }

        [Fact]
        public void DisposeMethodHappyPath()
        {
            var mockSender = new Mock<ISender>();
            Action<SenderMessage> validate = message => { };

            var validatingSender = new ValidatingSender("Foo", mockSender.Object, validate);

            validatingSender.Dispose();

            mockSender.Verify(m => m.Dispose(), Times.Once());
        }

        [Fact]
        public async Task SendAsyncMethodHappyPath()
        {
            var sentMessages = new List<SenderMessage>();

            var mockSender = new Mock<ISender>();
            Action<SenderMessage> validate = message => sentMessages.Add(message);

            using var validatingSender = new ValidatingSender("Foo", mockSender.Object, validate);

            var message1 = new SenderMessage("Hello, world!");
            var message2 = new SenderMessage("Good-bye, cruel world!");

            await validatingSender.SendAsync(message1).ConfigureAwait(false);
            await validatingSender.SendAsync(message2).ConfigureAwait(false);

            mockSender.Verify(m => m.SendAsync(message1, default), Times.Once());
            mockSender.Verify(m => m.SendAsync(message2, default), Times.Once());

            sentMessages.Should().HaveCount(2);
            sentMessages[0].Should().Be(message1);
            sentMessages[1].Should().Be(message2);
        }
    }
}

using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ValidatingTransactionalSenderTests
    {
        [Fact]
        public void ConstructorHappyPath()
        {
            var transactionalSender = new Mock<ITransactionalSender>().Object;
            Action<SenderMessage> validate = message => { };

            var validatingTransactionalSender = new ValidatingTransactionalSender("Foo", transactionalSender, validate);

            validatingTransactionalSender.Name.Should().Be("Foo");
            validatingTransactionalSender.Sender.Should().BeSameAs(transactionalSender);
            validatingTransactionalSender.TransactionalSender.Should().BeSameAs(transactionalSender);
            validatingTransactionalSender.Validate.Should().BeSameAs(validate);
        }

        [Fact]
        public void ConstructorSadPath1()
        {
            ITransactionalSender transactionalSender = new Mock<ITransactionalSender>().Object;
            Action<SenderMessage> validate = message => { };

            Action act = () => new ValidatingTransactionalSender(null, transactionalSender, validate);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*name*");
        }

        [Fact]
        public void ConstructorSadPath2()
        {
            ITransactionalSender transactionalSender = null;
            Action<SenderMessage> validate = message => { };

            Action act = () => new ValidatingTransactionalSender("Foo", transactionalSender, validate);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*sender*");
        }

        [Fact]
        public void ConstructorSadPath3()
        {
            ITransactionalSender transactionalSender = new Mock<ITransactionalSender>().Object;
            Action<SenderMessage> validate = null;

            Action act = () => new ValidatingTransactionalSender("Foo", transactionalSender, validate);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*validate*");
        }

        [Fact]
        public void BeginTransactionMethodHappyPath()
        {
            var senderTransaction = new Mock<ISenderTransaction>().Object;

            var mockTransactionalSender = new Mock<ITransactionalSender>();
            mockTransactionalSender.Setup(m => m.BeginTransaction()).Returns(senderTransaction);
            Action<SenderMessage> validate = message => { };

            var validatingTransactionalSender = new ValidatingTransactionalSender("Foo", mockTransactionalSender.Object, validate);

            var actualTransaction = validatingTransactionalSender.BeginTransaction();

            var validatingSenderTransaction =
                actualTransaction.Should().BeOfType<ValidatingSenderTransaction>().Subject;

            validatingSenderTransaction.Transaction.Should().BeSameAs(senderTransaction);
            validatingSenderTransaction.Validate.Should().BeSameAs(validate);
        }
    }
}

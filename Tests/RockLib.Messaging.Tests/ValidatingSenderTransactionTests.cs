using FluentAssertions;
using Moq;
using RockLib.Dynamic;
using System;
using System.Collections.Generic;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ValidatingSenderTransactionTests
    {
        [Fact]
        public void AddMethodHappyPath()
        {
            var sentMessages = new List<SenderMessage>();

            var mockTransaction = new Mock<ISenderTransaction>();
            Action<SenderMessage> validate = message => sentMessages.Add(message);

            ValidatingSenderTransaction validatingTransaction = typeof(ValidatingSenderTransaction).New(mockTransaction.Object, validate);

            var message1 = new SenderMessage("Hello, world!");
            var message2 = new SenderMessage("Good-bye, cruel world!");

            validatingTransaction.Add(message1);
            validatingTransaction.Add(message2);

            mockTransaction.Verify(m => m.Add(message1), Times.Once());
            mockTransaction.Verify(m => m.Add(message2), Times.Once());

            sentMessages.Should().HaveCount(2);
            sentMessages[0].Should().Be(message1);
            sentMessages[1].Should().Be(message2);
        }

        [Fact]
        public void CommitMethodHappyPath()
        {
            var mockTransaction = new Mock<ISenderTransaction>();
            Action<SenderMessage> validate = message => { };

            ValidatingSenderTransaction validatingTransaction = typeof(ValidatingSenderTransaction).New(mockTransaction.Object, validate);

            validatingTransaction.Commit();

            mockTransaction.Verify(m => m.Commit(), Times.Once());
        }

        [Fact]
        public void RollbackMethodHappyPath()
        {
            var mockTransaction = new Mock<ISenderTransaction>();
            Action<SenderMessage> validate = message => { };

            ValidatingSenderTransaction validatingTransaction = typeof(ValidatingSenderTransaction).New(mockTransaction.Object, validate);

            validatingTransaction.Rollback();

            mockTransaction.Verify(m => m.Rollback(), Times.Once());
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using RockLib.Messaging.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    public class DependencyInjectionTests
    {
        [Test]
        public void SenderTest()
        {
            var services = new ServiceCollection();

            var registeredSender = GetMockSender().Object;

            services.AddSender(sp => registeredSender);

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeSameAs(registeredSender);

            var senderLookup = serviceProvider.GetRequiredService<SenderLookup>();

            senderLookup("mySender").Should().BeSameAs(sender);
        }

        [Test]
        public void SenderDecoratorTest()
        {
            var services = new ServiceCollection();

            var registeredSender = GetMockSender().Object;

            services.AddSender(sp => registeredSender)
                .AddDecorator((s, sp) => new SenderDecorator(s));

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType<SenderDecorator>()
                .Which.Sender.Should().BeSameAs(registeredSender);
        }

        private class SenderDecorator : ISender
        {
            public SenderDecorator(ISender sender) => Sender = sender;
            public ISender Sender { get; }
            public string Name => Sender.Name;
            public void Dispose() => Sender.Dispose();
            public Task SendAsync(SenderMessage message, CancellationToken cancellationToken) => Sender.SendAsync(message, cancellationToken);
        }

        [Test]
        public void TransactionalSenderTest()
        {
            var services = new ServiceCollection();

            var registeredTransactionalSender = GetMockTransactionalSender().Object;

            services.AddTransactionalSender(sp => registeredTransactionalSender);

            var serviceProvider = services.BuildServiceProvider();

            var transactionalSender = serviceProvider.GetRequiredService<ITransactionalSender>();

            transactionalSender.Should().BeSameAs(registeredTransactionalSender);

            var transactionalSenderLookup = serviceProvider.GetRequiredService<TransactionalSenderLookup>();
            var senderLookup = serviceProvider.GetRequiredService<SenderLookup>();

            transactionalSenderLookup("myTransactionalSender").Should().BeSameAs(transactionalSender);
            senderLookup("myTransactionalSender").Should().BeSameAs(transactionalSender);
        }

        [Test]
        public void TransactionalSenderDecoratorTest()
        {
            var services = new ServiceCollection();

            var registeredSender = GetMockTransactionalSender().Object;

            services.AddTransactionalSender(sp => registeredSender)
                .AddDecorator((s, sp) => new TransactionalSenderDecorator(s));

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ITransactionalSender>();

            sender.Should().BeOfType<TransactionalSenderDecorator>()
                .Which.Sender.Should().BeSameAs(registeredSender);
        }

        private class TransactionalSenderDecorator : ITransactionalSender
        {
            public TransactionalSenderDecorator(ITransactionalSender sender) => Sender = sender;
            public ITransactionalSender Sender { get; }
            public string Name => Sender.Name;
            public ISenderTransaction BeginTransaction() => Sender.BeginTransaction();
            public void Dispose() => Sender.Dispose();
            public Task SendAsync(SenderMessage message, CancellationToken cancellationToken) => Sender.SendAsync(message, cancellationToken);
        }

        [Test]
        public void ReceiverTest()
        {
            var services = new ServiceCollection();

            var registeredReceiver = GetMockReceiver().Object;

            services.AddReceiver(sp => registeredReceiver);

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeSameAs(registeredReceiver);

            var receiverLookup = serviceProvider.GetRequiredService<ReceiverLookup>();

            receiverLookup("myReceiver").Should().BeSameAs(receiver);
        }

        [Test]
        public void ReceiverDecoratorTest()
        {
            var services = new ServiceCollection();

            var registeredReceiver = GetMockReceiver().Object;

            services.AddReceiver(sp => registeredReceiver)
                .AddDecorator((s, sp) => new ReceiverDecorator(s));

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType<ReceiverDecorator>()
                .Which.Receiver.Should().BeSameAs(registeredReceiver);
        }

        private class ReceiverDecorator : IReceiver
        {
            public ReceiverDecorator(IReceiver sender) => Receiver = sender;
            public IReceiver Receiver { get; }
            public IServiceProvider ServiceProvider { get; }
            public string Name => Receiver.Name;
            public IMessageHandler MessageHandler { get => Receiver.MessageHandler; set => Receiver.MessageHandler = value; }
            public event EventHandler Connected { add { Receiver.Connected += value; } remove { Receiver.Connected -= value; } }
            public event EventHandler<DisconnectedEventArgs> Disconnected { add { Receiver.Disconnected += value; } remove { Receiver.Disconnected -= value; } }
            public event EventHandler<ErrorEventArgs> Error { add { Receiver.Error += value; } remove { Receiver.Error -= value; } }
            public void Dispose() => Receiver.Dispose();
        }

        [Test]
        public void ForwardingReceiverTest()
        {
            var services = new ServiceCollection();

            const string acknowledgeForwarderName = "myAcknowledgeForwarder";
            const ForwardingOutcome acknowledgeOutcome = ForwardingOutcome.Reject;
            const string rollbackForwarderName = "myRollbackForwarder";
            const ForwardingOutcome rollbackOutcome = ForwardingOutcome.Acknowledge;
            const string rejectForwarderName = "myRejectForwarder";
            const ForwardingOutcome rejectOutcome = ForwardingOutcome.Rollback;

            var mockAcknowledgeForwarder = GetMockSender(acknowledgeForwarderName);
            var mockRollbackForwarder = GetMockSender(rollbackForwarderName);
            var mockRejectForwarder = GetMockSender(rejectForwarderName);
            var mockReceiver = GetMockReceiver();

            services.AddSender(sp => mockAcknowledgeForwarder.Object);
            services.AddSender(sp => mockRollbackForwarder.Object);
            services.AddSender(sp => mockRejectForwarder.Object);

            services.AddReceiver(sp => mockReceiver.Object)
                .AddForwardingReceiver(options =>
                {
                    options.AcknowledgeForwarderName = acknowledgeForwarderName;
                    options.AcknowledgeOutcome = acknowledgeOutcome;
                    options.RollbackForwarderName = rollbackForwarderName;
                    options.RollbackOutcome = rollbackOutcome;
                    options.RejectForwarderName = rejectForwarderName;
                    options.RejectOutcome = rejectOutcome;
                });

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            var forwardingReceiver = receiver.Should().BeOfType<ForwardingReceiver>().Subject;

            forwardingReceiver.Receiver.Should().BeSameAs(mockReceiver.Object);
            forwardingReceiver.AcknowledgeForwarder.Should().BeSameAs(mockAcknowledgeForwarder.Object);
            forwardingReceiver.AcknowledgeOutcome.Should().Be(acknowledgeOutcome);
            forwardingReceiver.RollbackForwarder.Should().BeSameAs(mockRollbackForwarder.Object);
            forwardingReceiver.RollbackOutcome.Should().Be(rollbackOutcome);
            forwardingReceiver.RejectForwarder.Should().BeSameAs(mockRejectForwarder.Object);
            forwardingReceiver.RejectOutcome.Should().Be(rejectOutcome);
        }

        private static Mock<ISender> GetMockSender(string senderName = "mySender")
        {
            var mockSender = new Mock<ISender>();
            mockSender.Setup(m => m.Name).Returns(senderName);
            return mockSender;
        }

        private static Mock<ITransactionalSender> GetMockTransactionalSender()
        {
            var mockTransactionalSender = new Mock<ITransactionalSender>();
            mockTransactionalSender.Setup(m => m.Name).Returns("myTransactionalSender");
            return mockTransactionalSender;
        }

        private static Mock<IReceiver> GetMockReceiver()
        {
            var mockReceiver = new Mock<IReceiver>();
            mockReceiver.Setup(m => m.Name).Returns("myReceiver");
            return mockReceiver;
        }
    }
}

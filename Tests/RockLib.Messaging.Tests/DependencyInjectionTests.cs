using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RockLib.Messaging.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
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

        [Fact(DisplayName = "Resolved (transactional) senders that are not singleton and are not selected are disposed")]
        public void SenderLookupTest()
        {
            var mockSenderSingleton = GetMockSender("singleton");
            var mockSenderTransient = GetMockSender("transient");
            var mockSenderScoped = GetMockSender("scoped");

            var mockTransactionalSenderSingleton = GetMockTransactionalSender("singletonTransactional");
            var mockTransactionalSenderTransient = GetMockTransactionalSender("transientTransactional");
            var mockTransactionalSenderScoped = GetMockTransactionalSender("scopedTransactional");

            var services = new ServiceCollection();

            services.AddSender(sp => mockSenderSingleton.Object, ServiceLifetime.Singleton);
            services.AddSender(sp => mockSenderTransient.Object, ServiceLifetime.Transient);
            services.AddSender(sp => mockSenderScoped.Object, ServiceLifetime.Scoped);

            services.AddTransactionalSender(sp => mockTransactionalSenderSingleton.Object, ServiceLifetime.Singleton);
            services.AddTransactionalSender(sp => mockTransactionalSenderTransient.Object, ServiceLifetime.Transient);
            services.AddTransactionalSender(sp => mockTransactionalSenderScoped.Object, ServiceLifetime.Scoped);

            var serviceProvider = services.BuildServiceProvider();

            var senderLookup = serviceProvider.GetRequiredService<SenderLookup>();

            senderLookup("singleton").Should().BeSameAs(mockSenderSingleton.Object);

            mockSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockSenderScoped.Verify(m => m.Dispose(), Times.Once());

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Never());
            
            ClearInvocations();

            senderLookup("transient").Should().BeSameAs(mockSenderTransient.Object);

            mockSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockSenderTransient.Verify(m => m.Dispose(), Times.Never());
            mockSenderScoped.Verify(m => m.Dispose(), Times.Once());

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Never());

            ClearInvocations();

            senderLookup("scoped").Should().BeSameAs(mockSenderScoped.Object);

            mockSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockSenderScoped.Verify(m => m.Dispose(), Times.Never());

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Never());

            mockSenderSingleton.Invocations.Clear();
            mockSenderTransient.Invocations.Clear();
            mockSenderScoped.Invocations.Clear();

            senderLookup("singletonTransactional").Should().BeSameAs(mockTransactionalSenderSingleton.Object);

            mockSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockSenderScoped.Verify(m => m.Dispose(), Times.Once());

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Once());

            ClearInvocations();

            senderLookup("transientTransactional").Should().BeSameAs(mockTransactionalSenderTransient.Object);

            mockSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockSenderScoped.Verify(m => m.Dispose(), Times.Once());

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Once());

            ClearInvocations();

            senderLookup("scopedTransactional").Should().BeSameAs(mockTransactionalSenderScoped.Object);

            mockSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockSenderScoped.Verify(m => m.Dispose(), Times.Once());

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Never());

            void ClearInvocations()
            {
                mockTransactionalSenderSingleton.Invocations.Clear();
                mockTransactionalSenderTransient.Invocations.Clear();
                mockTransactionalSenderScoped.Invocations.Clear();

                mockSenderSingleton.Invocations.Clear();
                mockSenderTransient.Invocations.Clear();
                mockSenderScoped.Invocations.Clear();
            }
        }

        [Fact]
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

        [Fact]
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

        [Fact(DisplayName = "Resolved transactional senders that are not singleton and are not selected are disposed")]
        public void TransactionalSenderLookupTest()
        {
            var mockTransactionalSenderSingleton = GetMockTransactionalSender("singleton");
            var mockTransactionalSenderTransient = GetMockTransactionalSender("transient");
            var mockTransactionalSenderScoped = GetMockTransactionalSender("scoped");

            var services = new ServiceCollection();

            services.AddTransactionalSender(sp => mockTransactionalSenderSingleton.Object, ServiceLifetime.Singleton);
            services.AddTransactionalSender(sp => mockTransactionalSenderTransient.Object, ServiceLifetime.Transient);
            services.AddTransactionalSender(sp => mockTransactionalSenderScoped.Object, ServiceLifetime.Scoped);

            var serviceProvider = services.BuildServiceProvider();

            var transactionalSenderLookup = serviceProvider.GetRequiredService<TransactionalSenderLookup>();

            transactionalSenderLookup("singleton").Should().BeSameAs(mockTransactionalSenderSingleton.Object);

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Once());
            
            ClearInvocations();

            transactionalSenderLookup("transient").Should().BeSameAs(mockTransactionalSenderTransient.Object);

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Once());

            ClearInvocations();

            transactionalSenderLookup("scoped").Should().BeSameAs(mockTransactionalSenderScoped.Object);

            mockTransactionalSenderSingleton.Verify(m => m.Dispose(), Times.Never());
            mockTransactionalSenderTransient.Verify(m => m.Dispose(), Times.Once());
            mockTransactionalSenderScoped.Verify(m => m.Dispose(), Times.Never());

            void ClearInvocations()
            {
                mockTransactionalSenderSingleton.Invocations.Clear();
                mockTransactionalSenderTransient.Invocations.Clear();
                mockTransactionalSenderScoped.Invocations.Clear();
            }
        }

        [Fact]
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

        [Fact]
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

        [Fact(DisplayName = "Resolved receivers that are not singleton and are not selected are disposed")]
        public void ReceiverLookupTest()
        {
            var mockReceiverSingleton = GetMockReceiver("singleton");
            var mockReceiverTransient = GetMockReceiver("transient");
            var mockReceiverScoped = GetMockReceiver("scoped");

            var services = new ServiceCollection();

            services.AddReceiver(sp => mockReceiverSingleton.Object, ServiceLifetime.Singleton);
            services.AddReceiver(sp => mockReceiverTransient.Object, ServiceLifetime.Transient);
            services.AddReceiver(sp => mockReceiverScoped.Object, ServiceLifetime.Scoped);

            var serviceProvider = services.BuildServiceProvider();

            var receiverLookup = serviceProvider.GetRequiredService<ReceiverLookup>();

            receiverLookup("singleton").Should().BeSameAs(mockReceiverSingleton.Object);

            mockReceiverSingleton.Verify(m => m.Dispose(), Times.Never());
            mockReceiverTransient.Verify(m => m.Dispose(), Times.Once());
            mockReceiverScoped.Verify(m => m.Dispose(), Times.Once());

            ClearInvocations();

            receiverLookup("transient").Should().BeSameAs(mockReceiverTransient.Object);

            mockReceiverSingleton.Verify(m => m.Dispose(), Times.Never());
            mockReceiverTransient.Verify(m => m.Dispose(), Times.Never());
            mockReceiverScoped.Verify(m => m.Dispose(), Times.Once());

            ClearInvocations();

            receiverLookup("scoped").Should().BeSameAs(mockReceiverScoped.Object);

            mockReceiverSingleton.Verify(m => m.Dispose(), Times.Never());
            mockReceiverTransient.Verify(m => m.Dispose(), Times.Once());
            mockReceiverScoped.Verify(m => m.Dispose(), Times.Never());

            void ClearInvocations()
            {
                mockReceiverSingleton.Invocations.Clear();
                mockReceiverTransient.Invocations.Clear();
                mockReceiverScoped.Invocations.Clear();
            }
        }

        [Fact]
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

        [Fact]
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

        private static Mock<ITransactionalSender> GetMockTransactionalSender(string transactionalSenderName = "myTransactionalSender")
        {
            var mockTransactionalSender = new Mock<ITransactionalSender>();
            mockTransactionalSender.Setup(m => m.Name).Returns(transactionalSenderName);
            return mockTransactionalSender;
        }

        private static Mock<IReceiver> GetMockReceiver(string receiverName = "myReceiver")
        {
            var mockReceiver = new Mock<IReceiver>();
            mockReceiver.Setup(m => m.Name).Returns(receiverName);
            return mockReceiver;
        }
    }
}

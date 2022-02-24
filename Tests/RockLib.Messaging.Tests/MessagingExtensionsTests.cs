using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using RockLib.Configuration;
using RockLib.Messaging.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using static RockLib.Messaging.Tests.ReloadingSenderTests;
using static RockLib.Messaging.Tests.ReloadingReceiverTests;
using FluentAssertions.Extensions;

namespace RockLib.Messaging.Tests
{
    public class MessagingExtensionsTests
    {
        #region Method 1 - configured by configuration, created by configuration object factory

        [Fact]
        public void AddSenderExtensionMethod1HappyPath()
        {
            var services = new ServiceCollection();

            services.AddSingleton(Config.Root);

            services.AddSender("MyTestSenderDecorator");

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.ExecutionTimeOf(s => s.GetRequiredService<ISender>()).Should().BeLessOrEqualTo(250.Milliseconds());
        }

        [Fact]
        public void AddReceiverExtensionMethod1HappyPath()
        {
            var services = new ServiceCollection();

            services.AddSingleton(Config.Root);

            services.AddReceiver("MyTestReceiverDecorator");

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.ExecutionTimeOf(s => s.GetRequiredService<IReceiver>()).Should().BeLessOrEqualTo(250.Milliseconds());
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public sealed class TestSenderDecorator : ISender
        {
            public TestSenderDecorator(string name, ISender sender) =>
                (Name, Sender) = (name, sender);

            public TestSenderDecorator(string name, string senderName)
#pragma warning disable CA2000 // Dispose objects before losing scope
					 : this(name, MessagingScenarioFactory.CreateSender(senderName))
#pragma warning restore CA2000 // Dispose objects before losing scope
			{
            }

            public string Name { get; }

            public ISender Sender { get; }

            public void Dispose() => Sender.Dispose();

            public Task SendAsync(SenderMessage message, CancellationToken cancellationToken) =>
                Sender.SendAsync(message, cancellationToken);
        }

        public sealed class TestReceiverDecorator : IReceiver
        {
            public TestReceiverDecorator(string name, IReceiver receiver) =>
                (Name, Receiver) = (name, receiver);

            public TestReceiverDecorator(string name, string receiverName)
#pragma warning disable CA2000 // Dispose objects before losing scope
					 : this(name, MessagingScenarioFactory.CreateReceiver(receiverName))
#pragma warning restore CA2000 // Dispose objects before losing scope
			{
            }

            public string Name { get; }

            public IReceiver Receiver { get; }

            public IMessageHandler MessageHandler
            {
                get => Receiver.MessageHandler;
                set => Receiver.MessageHandler = value;
            }

            public event EventHandler Connected
            {
                add { Receiver.Connected += value; }
                remove { Receiver.Connected -= value; }
            }

            public event EventHandler<DisconnectedEventArgs> Disconnected
            {
                add { Receiver.Disconnected += value; }
                remove { Receiver.Disconnected -= value; }
            }

            public event EventHandler<ErrorEventArgs> Error
            {
                add { Receiver.Error += value; }
                remove { Receiver.Error -= value; }
            }

            public void Dispose() => Receiver.Dispose();
        }
#pragma warning restore CA1034 // Nested types should not be visible

        #endregion

        #region Method 2 - configured by options, created by callback

        #region Reloading sender/receiver vs regular sender/receiver

        [Fact(DisplayName = "AddSender extension method 2 adds reloading sender when reloadOnChange parameter is true")]
        public void AddSenderExtensionMethod2HappyPath1()
        {
            var services = new ServiceCollection();

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>();
            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddSender<TestSenderOptions>("MyReloadingSender",
                (o, p) => new TestSender("MyTestSender", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType(ReloadingSender);

            ISender innerSender = ((dynamic)sender).Sender;

            innerSender.Should().BeOfType<TestSender>();
        }

        [Fact(DisplayName = "AddSender extension method 2 adds regular sender when reloadOnChange parameter is false")]
        public void AddSenderExtensionMethod2HappyPath2()
        {
            var services = new ServiceCollection();

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>();
            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddSender<TestSenderOptions>("MyReloadingSender",
                (o, p) => new TestSender("MyTestSender", o.TestSetting1, o.TestSetting2),
                reloadOnChange: false);

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType<TestSender>();
        }

        [Fact(DisplayName = "AddSender extension method 2 adds regular sender when reloadOnChange parameter is true but IOptionsMonitor is not available")]
        public void AddSenderExtensionMethod2HappyPath3()
        {
            var services = new ServiceCollection();

            services.AddSender<TestSenderOptions>("MyReloadingSender",
                (o, p) => new TestSender("MyTestSender", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType<TestSender>();
        }

        [Fact(DisplayName = "AddReceiver extension method 2 adds reloading receiver when reloadOnChange parameter is true")]
        public void AddReceiverExtensionMethod2HappyPath1()
        {
            var services = new ServiceCollection();

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>();
            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddReceiver<TestReceiverOptions>("MyReloadingReceiver",
                (o, p) => new TestReceiver("MyTestReceiver", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType(ReloadingReceiver);

            IReceiver innerReceiver = ((dynamic)receiver).Receiver;

            innerReceiver.Should().BeOfType<TestReceiver>();
        }

        [Fact(DisplayName = "AddReceiver extension method 2 adds regular receiver when reloadOnChange parameter is false")]
        public void AddReceiverExtensionMethod2HappyPath2()
        {
            var services = new ServiceCollection();

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>();
            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddReceiver<TestReceiverOptions>("MyReloadingReceiver",
                (o, p) => new TestReceiver("MyTestReceiver", o.TestSetting1, o.TestSetting2),
                reloadOnChange: false);

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType<TestReceiver>();
        }

        [Fact(DisplayName = "AddReceiver extension method 2 adds regular receiver when reloadOnChange parameter is true but IOptionsMonitor is not available")]
        public void AddReceiverExtensionMethod2HappyPath3()
        {
            var services = new ServiceCollection();

            services.AddReceiver<TestReceiverOptions>("MyReloadingReceiver",
                (o, p) => new TestReceiver("MyTestReceiver", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType<TestReceiver>();
        }

        #endregion

        #region How are options initialized?

        [Fact(DisplayName = "AddSender extension method 2 uses options from options monitor")]
        public void AddSenderExtensionMethod2HappyPath4()
        {
            var services = new ServiceCollection();

            var options = new TestSenderOptions
            {
                TestSetting1 = "OptionsMonitorTestSetting1",
                TestSetting2 = "OptionsMonitorTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("MyReloadingSender")).Returns(options);

            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddSender<TestSenderOptions>("MyReloadingSender",
                (o, p) => new TestSender("MyTestSender", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType(ReloadingSender);

            TestSender innerSender = ((dynamic)sender).Sender;

            innerSender.TestSetting1.Should().Be("OptionsMonitorTestSetting1");
            innerSender.TestSetting2.Should().Be("OptionsMonitorTestSetting2");
        }

        [Fact(DisplayName = "AddSender extension method 2 uses default options if options monitor is not available")]
        public void AddSenderExtensionMethod2HappyPath5()
        {
            var services = new ServiceCollection();

            services.AddSender<TestSenderOptions>("MyReloadingSender",
                (o, p) => new TestSender("MyTestSender", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var testSender =
                sender.Should().BeOfType<TestSender>()
                    .Subject;

            testSender.TestSetting1.Should().Be("DefaultTestSetting1");
            testSender.TestSetting2.Should().Be("DefaultTestSetting2");
        }

        [Fact(DisplayName = "AddSender extension method 2 uses default options if IOptionsMonitor.Get returns null")]
        public void AddSenderExtensionMethod2HappyPath6()
        {
            var services = new ServiceCollection();

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("MyReloadingSender")).Returns((TestSenderOptions)null!);

            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddSender<TestSenderOptions>("MyReloadingSender",
                (o, p) => new TestSender("MyTestSender", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType(ReloadingSender);

            TestSender innerSender = ((dynamic)sender).Sender;

            innerSender.TestSetting1.Should().Be("DefaultTestSetting1");
            innerSender.TestSetting2.Should().Be("DefaultTestSetting2");
        }

        [Fact(DisplayName = "AddSender extension method 2 applies configureOptions parameter last")]
        public void AddSenderExtensionMethod2HappyPath7()
        {
            var services = new ServiceCollection();

            var options = new TestSenderOptions
            {
                TestSetting1 = "OptionsMonitorTestSetting1",
                TestSetting2 = "OptionsMonitorTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("MyReloadingSender")).Returns(options);

            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddSender<TestSenderOptions>("MyReloadingSender",
                (o, p) => new TestSender("MyTestSender", o.TestSetting1, o.TestSetting2),
                o =>
                {
                    o.TestSetting1 = "ConfiguredTestSetting1";
                    o.TestSetting2 = "ConfiguredTestSetting2";
                });

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType(ReloadingSender);

            TestSender innerSender = ((dynamic)sender).Sender;

            innerSender.TestSetting1.Should().Be("ConfiguredTestSetting1");
            innerSender.TestSetting2.Should().Be("ConfiguredTestSetting2");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 uses options from options monitor")]
        public void AddReceiverExtensionMethod2HappyPath4()
        {
            var services = new ServiceCollection();

            var options = new TestReceiverOptions
            {
                TestSetting1 = "OptionsMonitorTestSetting1",
                TestSetting2 = "OptionsMonitorTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("MyReloadingReceiver")).Returns(options);

            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddReceiver<TestReceiverOptions>("MyReloadingReceiver",
                (o, p) => new TestReceiver("MyTestReceiver", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType(ReloadingReceiver);

            TestReceiver innerReceiver = ((dynamic)receiver).Receiver;

            innerReceiver.TestSetting1.Should().Be("OptionsMonitorTestSetting1");
            innerReceiver.TestSetting2.Should().Be("OptionsMonitorTestSetting2");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 uses default options if options monitor is not available")]
        public void AddReceiverExtensionMethod2HappyPath5()
        {
            var services = new ServiceCollection();

            services.AddReceiver<TestReceiverOptions>("MyReloadingReceiver",
                (o, p) => new TestReceiver("MyTestReceiver", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            var testReceiver =
                receiver.Should().BeOfType<TestReceiver>()
                    .Subject;

            testReceiver.TestSetting1.Should().Be("DefaultTestSetting1");
            testReceiver.TestSetting2.Should().Be("DefaultTestSetting2");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 uses default options if IOptionsMonitor.Get returns null")]
        public void AddReceiverExtensionMethod2HappyPath6()
        {
            var services = new ServiceCollection();

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("MyReloadingReceiver")).Returns((TestReceiverOptions)null!);

            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddReceiver<TestReceiverOptions>("MyReloadingReceiver",
                (o, p) => new TestReceiver("MyTestReceiver", o.TestSetting1, o.TestSetting2));

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType(ReloadingReceiver);

            TestReceiver innerReceiver = ((dynamic)receiver).Receiver;

            innerReceiver.TestSetting1.Should().Be("DefaultTestSetting1");
            innerReceiver.TestSetting2.Should().Be("DefaultTestSetting2");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 applies configureOptions parameter last")]
        public void AddReceiverExtensionMethod2HappyPath7()
        {
            var services = new ServiceCollection();

            var options = new TestReceiverOptions
            {
                TestSetting1 = "OptionsMonitorTestSetting1",
                TestSetting2 = "OptionsMonitorTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("MyReloadingReceiver")).Returns(options);

            services.AddSingleton(mockOptionsMonitor.Object);

            services.AddReceiver<TestReceiverOptions>("MyReloadingReceiver",
                (o, p) => new TestReceiver("MyTestReceiver", o.TestSetting1, o.TestSetting2),
                o =>
                {
                    o.TestSetting1 = "ConfiguredTestSetting1";
                    o.TestSetting2 = "ConfiguredTestSetting2";
                });

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType(ReloadingReceiver);

            TestReceiver innerReceiver = ((dynamic)receiver).Receiver;

            innerReceiver.TestSetting1.Should().Be("ConfiguredTestSetting1");
            innerReceiver.TestSetting2.Should().Be("ConfiguredTestSetting2");
        }

        #endregion

        [Fact(DisplayName = "AddSender extension method 2 throws when services parameter is null")]
        public void AddSenderExtensionMethod2SadPath1()
        {
            IServiceCollection services = null!;
            string senderName = "MySender";
            Func<TestSenderOptions, IServiceProvider, ISender> createSender = (options, serviceProvider) => new TestSender("MyTestSender", options.TestSetting1, options.TestSetting2);

            Action act = () => services.AddSender(senderName, createSender);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*services*");
        }

        [Fact(DisplayName = "AddSender extension method 2 throws when senderName parameter is null")]
        public void AddSenderExtensionMethod2SadPath2()
        {
            IServiceCollection services = new ServiceCollection();
            string senderName = null!;
            Func<TestSenderOptions, IServiceProvider, ISender> createSender = (options, serviceProvider) => new TestSender("MyTestSender", options.TestSetting1, options.TestSetting2);

            Action act = () => services.AddSender(senderName, createSender);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*senderName*");
        }

        [Fact(DisplayName = "AddSender extension method 2 throws when createSender parameter is null")]
        public void AddSenderExtensionMethod2SadPath3()
        {
            IServiceCollection services = new ServiceCollection();
            string senderName = "MySender";
            Func<TestSenderOptions, IServiceProvider, ISender> createSender = null!;

            Action act = () => services.AddSender(senderName, createSender);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*createSender*");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 throws when services parameter is null")]
        public void AddReceiverExtensionMethod2SadPath1()
        {
            IServiceCollection services = null!;
            string receiverName = "MyReceiver";
            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = (options, serviceProvider) => new TestReceiver("MyTestReceiver", options.TestSetting1, options.TestSetting2);

            Action act = () => services.AddReceiver(receiverName, createReceiver);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*services*");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 throws when receiverName parameter is null")]
        public void AddReceiverExtensionMethod2SadPath2()
        {
            IServiceCollection services = new ServiceCollection();
            string receiverName = null!;
            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = (options, serviceProvider) => new TestReceiver("MyTestReceiver", options.TestSetting1, options.TestSetting2);

            Action act = () => services.AddReceiver(receiverName, createReceiver);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiverName*");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 throws when createReceiver parameter is null")]
        public void AddReceiverExtensionMethod2SadPath3()
        {
            IServiceCollection services = new ServiceCollection();
            string receiverName = "MyReceiver";
            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = null!;

            Action act = () => services.AddReceiver(receiverName, createReceiver);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*createReceiver*");
        }

        #endregion
    }
}

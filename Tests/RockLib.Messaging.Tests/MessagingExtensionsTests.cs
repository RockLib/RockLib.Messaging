using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using RockLib.Messaging.DependencyInjection;
using System;
using Xunit;
using static RockLib.Messaging.Tests.ReloadingSenderTests;
using static RockLib.Messaging.Tests.ReloadingReceiverTests;

namespace RockLib.Messaging.Tests
{
    public class MessagingExtensionsTests
    {
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
            mockOptionsMonitor.Setup(m => m.Get("MyReloadingSender")).Returns((TestSenderOptions)null);

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
            mockOptionsMonitor.Setup(m => m.Get("MyReloadingReceiver")).Returns((TestReceiverOptions)null);

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
            IServiceCollection services = null;
            string senderName = "MySender";
            Func<TestSenderOptions, IServiceProvider, ISender> createSender = (options, serviceProvider) => new TestSender("MyTestSender", options.TestSetting1, options.TestSetting2);

            Action act = () => services.AddSender(senderName, createSender);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*services*");
        }

        [Fact(DisplayName = "AddSender extension method 2 throws when senderName parameter is null")]
        public void AddSenderExtensionMethod2SadPath2()
        {
            IServiceCollection services = new ServiceCollection();
            string senderName = null;
            Func<TestSenderOptions, IServiceProvider, ISender> createSender = (options, serviceProvider) => new TestSender("MyTestSender", options.TestSetting1, options.TestSetting2);

            Action act = () => services.AddSender(senderName, createSender);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*senderName*");
        }

        [Fact(DisplayName = "AddSender extension method 2 throws when createSender parameter is null")]
        public void AddSenderExtensionMethod2SadPath3()
        {
            IServiceCollection services = new ServiceCollection();
            string senderName = "MySender";
            Func<TestSenderOptions, IServiceProvider, ISender> createSender = null;

            Action act = () => services.AddSender(senderName, createSender);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*createSender*");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 throws when services parameter is null")]
        public void AddReceiverExtensionMethod2SadPath1()
        {
            IServiceCollection services = null;
            string receiverName = "MyReceiver";
            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = (options, serviceProvider) => new TestReceiver("MyTestReceiver", options.TestSetting1, options.TestSetting2);

            Action act = () => services.AddReceiver(receiverName, createReceiver);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*services*");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 throws when receiverName parameter is null")]
        public void AddReceiverExtensionMethod2SadPath2()
        {
            IServiceCollection services = new ServiceCollection();
            string receiverName = null;
            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = (options, serviceProvider) => new TestReceiver("MyTestReceiver", options.TestSetting1, options.TestSetting2);

            Action act = () => services.AddReceiver(receiverName, createReceiver);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiverName*");
        }

        [Fact(DisplayName = "AddReceiver extension method 2 throws when createReceiver parameter is null")]
        public void AddReceiverExtensionMethod2SadPath3()
        {
            IServiceCollection services = new ServiceCollection();
            string receiverName = "MyReceiver";
            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = null;

            Action act = () => services.AddReceiver(receiverName, createReceiver);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*createReceiver*");
        }
    }
}

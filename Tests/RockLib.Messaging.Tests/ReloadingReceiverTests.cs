using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using RockLib.Dynamic;
using System;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ReloadingReceiverTests
    {
        public static readonly Type ReloadingReceiver;

        static ReloadingReceiverTests()
        {
            var reloadingReceiverType = Type.GetType("RockLib.Messaging.DependencyInjection.ReloadingReceiver`1, RockLib.Messaging", true);
            ReloadingReceiver = reloadingReceiverType.MakeGenericType(typeof(TestReceiverOptions));
        }

        [Fact(DisplayName = "Constructor sets its properties")]
        public void ConstructorTest()
        {
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            Func<TestReceiverOptions, IServiceProvider, IReceiver> expectedCreateReceiver = (options, provider) =>
            {
                return new TestReceiver("MyTestReceiver", options.TestSetting1, options.TestSetting2);
            };

            var testOptions = new TestReceiverOptions
            {
                TestSetting1 = "MyTestSetting1",
                TestSetting2 = "MyTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>(MockBehavior.Strict);
            var mockChangeListener = new Mock<IDisposable>(MockBehavior.Strict);

            mockOptionsMonitor.Setup(m => m.OnChange(It.IsAny<Action<TestReceiverOptions, string>>()))
                .Returns(mockChangeListener.Object);

            Action<TestReceiverOptions> expectedConfigureOptions = options => { };

            IReceiver receiver = ReloadingReceiver.New(mockServiceProvider.Object, "MyReloadingReceiver", expectedCreateReceiver, testOptions, mockOptionsMonitor.Object, expectedConfigureOptions);

            receiver.Name.Should().Be("MyReloadingReceiver");
            receiver.GetType().Should().Be(ReloadingReceiver);

            dynamic r = receiver;

            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = r.CreateReceiver;
            createReceiver.Should().BeSameAs(expectedCreateReceiver);

            Action<TestReceiverOptions> configureOptions = r.ConfigureOptions;
            configureOptions.Should().BeSameAs(expectedConfigureOptions);

            TestReceiver testReceiver = r.Receiver;
            testReceiver.Name.Should().Be("MyTestReceiver");
            testReceiver.TestSetting1.Should().Be("MyTestSetting1");
            testReceiver.TestSetting2.Should().Be("MyTestSetting2");

            IDisposable changeListener = r.ChangeListener;
            changeListener.Should().BeSameAs(mockChangeListener.Object);
        }

        [Fact(DisplayName = "When options change, a new inner receiver is created, state is transferred from the old one, and the old one is disposed")]
        public void OnOptionsChangedTest1()
        {
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = (options, provider) =>
            {
                return new TestReceiver("MyTestReceiver", options.TestSetting1, options.TestSetting2);
            };

            var initialOptions = new TestReceiverOptions
            {
                TestSetting1 = "InitialTestSetting1",
                TestSetting2 = "InitialTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>(MockBehavior.Strict);
            var mockChangeListener = new Mock<IDisposable>(MockBehavior.Strict);

            Action<TestReceiverOptions, string> onChangeCallback = null;

            mockOptionsMonitor.Setup(m => m.OnChange(It.IsAny<Action<TestReceiverOptions, string>>()))
                .Callback<Action<TestReceiverOptions, string>>(onChange => onChangeCallback = onChange)
                .Returns(mockChangeListener.Object);

            Action<TestReceiverOptions> configureOptions = options =>
            {
                options.TestSetting2 = "ConfiguredTestSetting2";
            };

            IReceiver receiver = ReloadingReceiver.New(mockServiceProvider.Object, "MyReloadingReceiver", createReceiver, initialOptions, mockOptionsMonitor.Object, configureOptions);
            dynamic r = receiver;

            onChangeCallback.Should().NotBeNull();

            var messageHandler = new Mock<IMessageHandler>().Object;
            EventHandler connected = (sender, args) => { };
            EventHandler<DisconnectedEventArgs> disconnected = (sender, args) => { };
            EventHandler<ErrorEventArgs> error = (sender, args) => { };

            receiver.MessageHandler = messageHandler;
            receiver.Connected += connected;
            receiver.Disconnected += disconnected;
            receiver.Error += error;

            TestReceiver initialTestReceiver = r.Receiver;

            initialTestReceiver.MessageHandler.Should().BeSameAs(messageHandler);
            initialTestReceiver.ConnectedHandler.Should().BeSameAs(connected);
            initialTestReceiver.DisconnectedHandler.Should().BeSameAs(disconnected);
            initialTestReceiver.ErrorHandler.Should().BeSameAs(error);

            var newOptions = new TestReceiverOptions
            {
                TestSetting1 = "NewTestSetting1",

                // Note that we won't see this value below, since our
                // 'configureOptions' action above overrides this value.
                TestSetting2 = "NewTestSetting2"
            };

            // Simulate the change to our receiver:
            onChangeCallback(newOptions, "MyReloadingReceiver");

            TestReceiver newTestReceiver = r.Receiver;
            
            newTestReceiver.Should().NotBeSameAs(initialTestReceiver);
            
            newTestReceiver.Name.Should().Be("MyTestReceiver");
            newTestReceiver.TestSetting1.Should().Be("NewTestSetting1");
            newTestReceiver.TestSetting2.Should().Be("ConfiguredTestSetting2");

            newTestReceiver.MessageHandler.Should().BeSameAs(messageHandler);
            newTestReceiver.ConnectedHandler.Should().BeSameAs(connected);
            newTestReceiver.DisconnectedHandler.Should().BeSameAs(disconnected);
            newTestReceiver.ErrorHandler.Should().BeSameAs(error);

            initialTestReceiver.Disposed.Should().BeTrue();
        }

        [Fact(DisplayName = "When options with a different name change, the inner receiver is not recreated")]
        public void OnOptionsChangedTest2()
        {
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = (options, provider) =>
            {
                return new TestReceiver("MyTestReceiver", options.TestSetting1, options.TestSetting2);
            };

            var initialOptions = new TestReceiverOptions
            {
                TestSetting1 = "InitialTestSetting1",
                TestSetting2 = "InitialTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>(MockBehavior.Strict);
            var mockChangeListener = new Mock<IDisposable>(MockBehavior.Strict);

            Action<TestReceiverOptions, string> onChangeCallback = null;

            mockOptionsMonitor.Setup(m => m.OnChange(It.IsAny<Action<TestReceiverOptions, string>>()))
                .Callback<Action<TestReceiverOptions, string>>(onChange => onChangeCallback = onChange)
                .Returns(mockChangeListener.Object);

            Action<TestReceiverOptions> configureOptions = options => { };

            IReceiver receiver = ReloadingReceiver.New(mockServiceProvider.Object, "MyReloadingReceiver", createReceiver, initialOptions, mockOptionsMonitor.Object, configureOptions);
            dynamic r = receiver;

            onChangeCallback.Should().NotBeNull();

            TestReceiver initialTestReceiver = r.Receiver;

            var newOptions = new TestReceiverOptions
            {
                TestSetting1 = "NewTestSetting1",
                TestSetting2 = "NewTestSetting2"
            };

            // Simulate a change to some other receiver:
            onChangeCallback(newOptions, "SomeOtherReceiver");

            TestReceiver newTestReceiver = r.Receiver;

            newTestReceiver.Should().BeSameAs(initialTestReceiver);
            initialTestReceiver.Disposed.Should().BeFalse();
        }

        [Fact(DisplayName = "Dispose method disposes the change listener and the current receiver")]
        public void DisposeTest()
        {
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            Func<TestReceiverOptions, IServiceProvider, IReceiver> createReceiver = (options, provider) =>
            {
                return new TestReceiver("MyTestReceiver", options.TestSetting1, options.TestSetting2);
            };

            var initialOptions = new TestReceiverOptions
            {
                TestSetting1 = "InitialTestSetting1",
                TestSetting2 = "InitialTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestReceiverOptions>>(MockBehavior.Strict);
            var mockChangeListener = new Mock<IDisposable>();

            mockOptionsMonitor.Setup(m => m.OnChange(It.IsAny<Action<TestReceiverOptions, string>>()))
                .Returns(mockChangeListener.Object);

            Action<TestReceiverOptions> configureOptions = options => { };
            
            IReceiver receiver = ReloadingReceiver.New(mockServiceProvider.Object, "MyReloadingReceiver", createReceiver, initialOptions, mockOptionsMonitor.Object, configureOptions);

            TestReceiver testReceiver = ((dynamic)receiver).Receiver;

            testReceiver.Disposed.Should().BeFalse();
            mockChangeListener.Verify(m => m.Dispose(), Times.Never());

            receiver.Dispose();

            testReceiver.Disposed.Should().BeTrue();
            mockChangeListener.Verify(m => m.Dispose());
        }

        public class TestReceiverOptions
        {
            public string TestSetting1 { get; set; } = "DefaultTestSetting1";

            public string TestSetting2 { get; set; } = "DefaultTestSetting2";
        }

        public sealed class TestReceiver : IReceiver
        {
            public TestReceiver(string name, string testSetting1, string testSetting2)
            {
                Name = name;
                TestSetting1 = testSetting1;
                TestSetting2 = testSetting2;
            }

            public string Name { get; }

            public string TestSetting1 { get; }

            public string TestSetting2 { get; }

            public IMessageHandler MessageHandler { get; set; }

            public event EventHandler Connected
            {
                add { ConnectedHandler += value; }
                remove { ConnectedHandler -= value; }
            }

            public event EventHandler<DisconnectedEventArgs> Disconnected
            {
                add { DisconnectedHandler += value; }
                remove { DisconnectedHandler -= value; }
            }

            public event EventHandler<ErrorEventArgs> Error
            {
                add { ErrorHandler += value; }
                remove { ErrorHandler -= value; }
            }

            public EventHandler ConnectedHandler { get; private set; }

            public EventHandler<DisconnectedEventArgs> DisconnectedHandler { get; private set; }

            public EventHandler<ErrorEventArgs> ErrorHandler { get; private set; }

            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}

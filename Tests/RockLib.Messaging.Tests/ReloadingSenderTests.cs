using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using RockLib.Dynamic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class ReloadingSenderTests
    {
        public static readonly Type ReloadingSender;

        static ReloadingSenderTests()
        {
            var reloadingSenderType = Type.GetType("RockLib.Messaging.DependencyInjection.ReloadingSender`1, RockLib.Messaging", true);
            ReloadingSender = reloadingSenderType.MakeGenericType(typeof(TestSenderOptions));
        }

        [Fact(DisplayName = "Constructor sets its properties")]
        public void ConstructorTest()
        {
            Func<TestSenderOptions, ISender> expectedCreateSender = options =>
            {
                return new TestSender("MyTestSender", options.TestSetting1, options.TestSetting2);
            };

            var testOptions = new TestSenderOptions
            {
                TestSetting1 = "MyTestSetting1",
                TestSetting2 = "MyTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>(MockBehavior.Strict);
            var mockChangeListener = new Mock<IDisposable>(MockBehavior.Strict);

            mockOptionsMonitor.Setup(m => m.Get("MyReloadingSender")).Returns(testOptions);
            mockOptionsMonitor.Setup(m => m.OnChange(It.IsAny<Action<TestSenderOptions, string>>()))
                .Returns(mockChangeListener.Object);

            Action<TestSenderOptions> expectedConfigureOptions = options => { };

            ISender sender = ReloadingSender.New("MyReloadingSender",
                expectedCreateSender, mockOptionsMonitor.Object, expectedConfigureOptions);

            sender.Name.Should().Be("MyReloadingSender");
            sender.GetType().Should().Be(ReloadingSender);

            dynamic s = sender;

            Func<TestSenderOptions, ISender> createSender = s.CreateSender;
            createSender.Should().BeSameAs(expectedCreateSender);

            Action<TestSenderOptions> configureOptions = s.ConfigureOptions;
            configureOptions.Should().BeSameAs(expectedConfigureOptions);

            TestSender testSender = s.Sender;
            testSender.Name.Should().Be("MyTestSender");
            testSender.TestSetting1.Should().Be("MyTestSetting1");
            testSender.TestSetting2.Should().Be("MyTestSetting2");

            IDisposable changeListener = s.ChangeListener;
            changeListener.Should().BeSameAs(mockChangeListener.Object);
        }

        [Fact(DisplayName = "When options change, a new inner sender is created and the old one is disposed")]
        public void OnOptionsChangedTest1()
        {
            Func<TestSenderOptions, ISender> createSender = options =>
            {
                return new TestSender("MyTestSender", options.TestSetting1, options.TestSetting2);
            };

            var initialOptions = new TestSenderOptions
            {
                TestSetting1 = "InitialTestSetting1",
                TestSetting2 = "InitialTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>(MockBehavior.Strict);
            var mockChangeListener = new Mock<IDisposable>(MockBehavior.Strict);

            Action<TestSenderOptions, string> onChangeCallback = null;

            mockOptionsMonitor.Setup(m => m.Get("MyReloadingSender")).Returns(initialOptions);
            mockOptionsMonitor.Setup(m => m.OnChange(It.IsAny<Action<TestSenderOptions, string>>()))
                .Callback<Action<TestSenderOptions, string>>(onChange => onChangeCallback = onChange)
                .Returns(mockChangeListener.Object);

            Action<TestSenderOptions> configureOptions = options =>
            {
                options.TestSetting2 = "ConfiguredTestSetting2";
            };

            ISender sender = ReloadingSender.New("MyReloadingSender",
                createSender, mockOptionsMonitor.Object, configureOptions);

            dynamic s = sender;

            onChangeCallback.Should().NotBeNull();

            TestSender initialTestSender = s.Sender;

            var newOptions = new TestSenderOptions
            {
                TestSetting1 = "NewTestSetting1",

                // Note that we won't see this value below, since our
                // 'configureOptions' action above overrides this value.
                TestSetting2 = "NewTestSetting2"
            };

            // Simulate the change to our sender:
            onChangeCallback(newOptions, "MyReloadingSender");

            TestSender newTestSender = s.Sender;

            newTestSender.Should().NotBeSameAs(initialTestSender);

            newTestSender.Name.Should().Be("MyTestSender");
            newTestSender.TestSetting1.Should().Be("NewTestSetting1");
            newTestSender.TestSetting2.Should().Be("ConfiguredTestSetting2");

            initialTestSender.Disposed.Should().BeTrue();
        }

        [Fact(DisplayName = "When options with a different name change, the inner sender is not recreated")]
        public void OnOptionsChangedTest2()
        {
            Func<TestSenderOptions, ISender> createSender = options =>
            {
                return new TestSender("MyTestSender", options.TestSetting1, options.TestSetting2);
            };

            var initialOptions = new TestSenderOptions
            {
                TestSetting1 = "InitialTestSetting1",
                TestSetting2 = "InitialTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>(MockBehavior.Strict);
            var mockChangeListener = new Mock<IDisposable>(MockBehavior.Strict);

            Action<TestSenderOptions, string> onChangeCallback = null;

            mockOptionsMonitor.Setup(m => m.Get("MyReloadingSender")).Returns(initialOptions);
            mockOptionsMonitor.Setup(m => m.OnChange(It.IsAny<Action<TestSenderOptions, string>>()))
                .Callback<Action<TestSenderOptions, string>>(onChange => onChangeCallback = onChange)
                .Returns(mockChangeListener.Object);

            Action<TestSenderOptions> configureOptions = options => { };

            ISender sender = ReloadingSender.New("MyReloadingSender",
                createSender, mockOptionsMonitor.Object, configureOptions);

            dynamic r = sender;

            onChangeCallback.Should().NotBeNull();

            TestSender initialTestSender = r.Sender;

            var newOptions = new TestSenderOptions
            {
                TestSetting1 = "NewTestSetting1",
                TestSetting2 = "NewTestSetting2"
            };

            // Simulate a change to some other sender:
            onChangeCallback(newOptions, "SomeOtherSender");

            TestSender newTestSender = r.Sender;

            newTestSender.Should().BeSameAs(initialTestSender);
            initialTestSender.Disposed.Should().BeFalse();
        }

        [Fact(DisplayName = "Dispose method disposes the change listener and the current sender")]
        public void DisposeTest()
        {
            Func<TestSenderOptions, ISender> createSender = options =>
            {
                return new TestSender("MyTestSender", options.TestSetting1, options.TestSetting2);
            };

            var initialOptions = new TestSenderOptions
            {
                TestSetting1 = "InitialTestSetting1",
                TestSetting2 = "InitialTestSetting2"
            };

            var mockOptionsMonitor = new Mock<IOptionsMonitor<TestSenderOptions>>(MockBehavior.Strict);
            var mockChangeListener = new Mock<IDisposable>();

            mockOptionsMonitor.Setup(m => m.Get("MyReloadingSender")).Returns(initialOptions);
            mockOptionsMonitor.Setup(m => m.OnChange(It.IsAny<Action<TestSenderOptions, string>>()))
                .Returns(mockChangeListener.Object);

            Action<TestSenderOptions> configureOptions = options => { };

            ISender sender = ReloadingSender.New("MyReloadingSender",
                createSender, mockOptionsMonitor.Object, configureOptions);

            TestSender testSender = ((dynamic)sender).Sender;

            testSender.Disposed.Should().BeFalse();
            mockChangeListener.Verify(m => m.Dispose(), Times.Never());

            sender.Dispose();

            testSender.Disposed.Should().BeTrue();
            mockChangeListener.Verify(m => m.Dispose());
        }

        public class TestSenderOptions
        {
            public string TestSetting1 { get; set; } = "DefaultTestSetting1";

            public string TestSetting2 { get; set; } = "DefaultTestSetting2";
        }

        public sealed class TestSender : ISender
        {
            public TestSender(string name, string testSetting1, string testSetting2)
            {
                Name = name;
                TestSetting1 = testSetting1;
                TestSetting2 = testSetting2;
            }

            public string Name { get; }

            public string TestSetting1 { get; }

            public string TestSetting2 { get; }

            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }

            public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ObjectFactory;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class MessagingScenarioFactoryTests
    {
        [Fact]
        public void CreateSenderCreatesSenderWithSingleSenderConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\SingleSender_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            using var pipe1Sender = config.CreateSender("Pipe1");
            var sender = (FakeSender)((ConfigReloadingProxy<ISender>)pipe1Sender).Object;

            sender.Name.Should().Be("Pipe1");
            sender.PipeName.Should().Be("PipeName1");
        }

        [Fact]
        public void CreateSenderCreatesSendersWithMultipleSendersConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\MultipleSenders_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            using var pipe1Sender = config.CreateSender("Pipe1");
            var sender1 = (FakeSender)((ConfigReloadingProxy<ISender>)pipe1Sender).Object;

            sender1.Name.Should().Be("Pipe1");
            sender1.PipeName.Should().Be("PipeName1");

            using var pipe2Sender = config.CreateSender("Pipe2");
            var sender2 = (FakeSender)((ConfigReloadingProxy<ISender>)pipe2Sender).Object;

            sender2.Name.Should().Be("Pipe2");
            sender2.PipeName.Should().Be("PipeName2");
        }

        [Fact]
        public void CreateReceiverCreatesReceiverWithSingleReceiverConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\SingleReceiver_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            using var pipe1Sender = config.CreateReceiver("Pipe1");
            var receiver = (FakeReceiver)((ConfigReloadingProxy<IReceiver>)pipe1Sender).Object;

            receiver.Name.Should().Be("Pipe1");
            receiver.PipeName.Should().Be("PipeName1");
        }

        [Fact]
        public void CreateReceiverCreatesReceiversWithMultipleReceiversConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\MultipleReceivers_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            using var pipe1Sender = config.CreateReceiver("Pipe1");
            var receiver1 = (FakeReceiver)((ConfigReloadingProxy<IReceiver>)pipe1Sender).Object;

            receiver1.Name.Should().Be("Pipe1");
            receiver1.PipeName.Should().Be("PipeName1");

            using var pipe2Sender = config.CreateReceiver("Pipe2");
            var receiver2 = (FakeReceiver)((ConfigReloadingProxy<IReceiver>)pipe2Sender).Object;

            receiver2.Name.Should().Be("Pipe2");
            receiver2.PipeName.Should().Be("PipeName2");
        }

        [Fact]
        public void DefaultTypesFunctionsProperly()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RockLib.Messaging:Senders:Name", "foo" },
                    { "RockLib.Messaging:Receivers:Name", "foo" }
                }).Build();

            var defaultTypes = new DefaultTypes
            {
                { typeof(ISender), typeof(TestSender) },
                { typeof(IReceiver), typeof(TestReceiver) }
            };

            var messagingSection = config.GetSection("RockLib.Messaging");

            using var sender = messagingSection.CreateSender("foo", defaultTypes: defaultTypes, reloadOnConfigChange: false);
            using var receiver = messagingSection.CreateReceiver("foo", defaultTypes: defaultTypes, reloadOnConfigChange: false);

            sender.Should().BeOfType<TestSender>();
            receiver.Should().BeOfType<TestReceiver>();
        }

        [Fact]
        public void ValueConvertersFunctionsProperly()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RockLib.Messaging:Senders:Type", typeof(TestSender).AssemblyQualifiedName! },
                    { "RockLib.Messaging:Senders:Value:Name", "foo" },
                    { "RockLib.Messaging:Senders:Value:Location", "2,3" },
                    { "RockLib.Messaging:Receivers:Type", typeof(TestReceiver).AssemblyQualifiedName! },
                    { "RockLib.Messaging:Receivers:Value:Name", "foo" },
                    { "RockLib.Messaging:Receivers:Value:Location", "3,4" },
                }).Build();

            Point ParsePoint(string value)
            {
                var split = value.Split(',');
                return new Point(int.Parse(split[0], CultureInfo.InvariantCulture), int.Parse(split[1], CultureInfo.InvariantCulture));
            }

            var valueConverters = new ValueConverters
            {
                { typeof(Point), ParsePoint }
            };

            var messagingSection = config.GetSection("RockLib.Messaging");

            using var sender = (TestSender)messagingSection.CreateSender("foo", valueConverters: valueConverters, reloadOnConfigChange: false);
            using var receiver = (TestReceiver)messagingSection.CreateReceiver("foo", valueConverters: valueConverters, reloadOnConfigChange: false);

            sender.Location.X.Should().Be(2);
            sender.Location.Y.Should().Be(3);

            receiver.Location.X.Should().Be(3);
            receiver.Location.Y.Should().Be(4);
        }

        [Fact]
        public void ResolverFunctionsProperly()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RockLib.Messaging:Senders:Type", typeof(TestSender).AssemblyQualifiedName! },
                    { "RockLib.Messaging:Senders:Value:Name", "foo" },
                    { "RockLib.Messaging:Receivers:Type", typeof(TestReceiver).AssemblyQualifiedName! },
                    { "RockLib.Messaging:Receivers:Value:Name", "foo" },
                }).Build();

            var dependency = new TestDependency();
            var resolver = new Resolver(t => dependency, t => t == typeof(ITestDependency));

            var messagingSection = config.GetSection("RockLib.Messaging");

            using var sender = (TestSender)messagingSection.CreateSender("foo", resolver: resolver, reloadOnConfigChange: false);
            using var receiver = (TestReceiver)messagingSection.CreateReceiver("foo", resolver: resolver, reloadOnConfigChange: false);

            sender.Dependency.Should().BeSameAs(dependency);
            receiver.Dependency.Should().BeSameAs(dependency);
        }

        [Fact]
        public void ReloadOnConfigChangeTrueFunctionsProperly()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RockLib.Messaging:Senders:Type", typeof(TestSender).AssemblyQualifiedName! },
                    { "RockLib.Messaging:Senders:Value:Name", "foo" },
                    { "RockLib.Messaging:Receivers:Type", typeof(TestReceiver).AssemblyQualifiedName! },
                    { "RockLib.Messaging:Receivers:Value:Name", "foo" },
                }).Build();

            var messagingSection = config.GetSection("RockLib.Messaging");

            using var sender = messagingSection.CreateSender("foo", reloadOnConfigChange: true);
            using var receiver = messagingSection.CreateReceiver("foo", reloadOnConfigChange: true);

            sender.Should().BeAssignableTo<ConfigReloadingProxy<ISender>>();
            receiver.Should().BeAssignableTo<ConfigReloadingProxy<IReceiver>>();
        }

        [Fact]
        public void ReloadOnConfigChangeFalseFunctionsProperly()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RockLib.Messaging:Senders:Type", typeof(TestSender).AssemblyQualifiedName! },
                    { "RockLib.Messaging:Senders:Value:Name", "foo" },
                    { "RockLib.Messaging:Receivers:Type", typeof(TestReceiver).AssemblyQualifiedName! },
                    { "RockLib.Messaging:Receivers:Value:Name", "foo" },
                }).Build();

            var messagingSection = config.GetSection("RockLib.Messaging");

            using var sender = messagingSection.CreateSender("foo", reloadOnConfigChange: false);
            using var receiver = messagingSection.CreateReceiver("foo", reloadOnConfigChange: false);

            sender.Should().BeOfType<TestSender>();
            receiver.Should().BeOfType<TestReceiver>();
        }

#pragma warning disable CA1812
        private sealed class TestReceiver : Receiver
        {
            public TestReceiver(Point location = default, ITestDependency? dependency = null)
                : base(nameof(TestReceiver))
            {
                Location = location;
                Dependency = dependency;
            }

            public Point Location { get; }

            public ITestDependency? Dependency { get; }

            protected override void Start()
            {
            }
        }

        private sealed class TestSender : ISender
        {
            public TestSender(Point location = default, ITestDependency? dependency = null)
            {
                Name = nameof(TestSender);
                Location = location;
                Dependency = dependency;
            }

            public string Name { get; }

            public Point Location { get; }

            public ITestDependency? Dependency { get; }

            public void Dispose()
            {
            }

            public Task SendAsync(SenderMessage message, CancellationToken cancellationToken) => Task.FromResult(0);
        }

        private struct Point
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }

            public int Y { get; }
        }

        private interface ITestDependency
        {
        }

        private sealed class TestDependency : ITestDependency
        {
        }
#pragma warning restore CA1812
    }
}

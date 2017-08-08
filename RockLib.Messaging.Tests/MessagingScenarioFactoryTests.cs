
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RockLib.Configuration;
using RockLib.Messaging.NamedPipes;
using Xunit;

namespace RockLib.Messaging.Tests
{
    public class MessagingScenarioFactoryTests
    {
        [Fact]
        public void BuildFactoryFromCore_FromJsonConfig_WillPullSingleFactory()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(@"CustomConfigFiles\SingleFactory_RockLib.config.json", optional: false, reloadOnChange: true);

            var config = builder.Build();

            Config.SetRoot(config);

            var factory = MessagingScenarioFactory.BuildFactory();

            factory.Should().NotBeNull();
            factory.Should().BeOfType<NamedPipeMessagingScenarioFactory>();
        }

        [Fact]
        public void BuildFactoryFromCore_FromJsonConfig_WillPullMutlipleFactory()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\MultipleFactory_RockLib.config.json", optional: false, reloadOnChange: true);

            var config = builder.Build();

            Config.SetRoot(config);

            var factory = MessagingScenarioFactory.BuildFactory();

            factory.Should().NotBeNull();
            factory.Should().BeOfType<CompositeMessagingScenarioFactory>();
        }

        [Fact]
        public void SetCurrent_WhenProvidedAFactory_WillSetItAsCurrent()
        {
            MessagingScenarioFactory.SetCurrent(new StubMessagingScenarioFactory());

            var currentScenarioFactory = MessagingScenarioFactory.Current;

            currentScenarioFactory.Should().BeOfType<StubMessagingScenarioFactory>();
        }

        [Fact]
        public void SetFallback_WillSetFallback_WillBeUsedWhenCreatingDefaultFactory()
        {
            MessagingScenarioFactory.SetFallback(new StubMessagingScenarioFactory());

            var currentScenarioFactory = MessagingScenarioFactory.Current;

            currentScenarioFactory.Should().BeOfType<StubMessagingScenarioFactory>();
        }
    }

    public class StubMessagingScenarioFactory : IMessagingScenarioFactory
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public ISender CreateQueueProducer(string name)
        {
            throw new System.NotImplementedException();
        }

        public IReceiver CreateQueueConsumer(string name)
        {
            throw new System.NotImplementedException();
        }

        public ISender CreateTopicPublisher(string name)
        {
            throw new System.NotImplementedException();
        }

        public IReceiver CreateTopicSubscriber(string name)
        {
            throw new System.NotImplementedException();
        }

        public bool HasScenario(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}

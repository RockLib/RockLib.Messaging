
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

            var factory = MessagingScenarioFactory.BuildFactoryForCore();

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

            var factory = MessagingScenarioFactory.BuildFactoryForCore();

            factory.Should().NotBeNull();
            factory.Should().BeOfType<CompositeMessagingScenarioFactory>();
        }
    }
}

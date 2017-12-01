using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using RockLib.Configuration;
using RockLib.Immutable;
using RockLib.Messaging.NamedPipes;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class MessagingScenarioFactoryTests
    {
        [Test]
        public void BuildFactoryCreatesSingleFactoryWithSingleFactoryConfig()
        {
            ResetFactory();
            ResetConfig();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\SingleFactory_appsettings.json", false, true);

            var config = builder.Build();

            Config.SetRoot(config);

            var factory = MessagingScenarioFactory.BuildFactory();

            factory.Should().NotBeNull();
            factory.Should().BeOfType<NamedPipeMessagingScenarioFactory>();
        }

        [Test]
        public void BuildFactoryCreatesCompositeFactoryWithMultiFactoryConfig()
        {
            ResetFactory();
            ResetConfig();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\MultipleFactory_appsettings.json", false, true);

            var config = builder.Build();

            Config.SetRoot(config);

            var factory = MessagingScenarioFactory.BuildFactory();

            factory.Should().NotBeNull();
            factory.Should().BeOfType<CompositeMessagingScenarioFactory>();
        }

        [Test]
        public void SetCurrentSetsCurrentField()
        {
            ResetFactory();
            ResetConfig();

            var mockFactory = new Mock<IMessagingScenarioFactory>();

            MessagingScenarioFactory.SetCurrent(mockFactory.Object);

            MessagingScenarioFactory.Current.Should().BeSameAs(mockFactory.Object);
        }

        [Test]
        public void CreateDefaultMessagingScenarioFactoryUsesFallbackWhenNoConfig()
        {
            ResetFactory();
            ResetConfig();

            var mockFactory = new Mock<IMessagingScenarioFactory>();

            MessagingScenarioFactory.SetFallback(mockFactory.Object);

            var factory = CallPrivateCreateDefault();

            factory.Should().BeSameAs(mockFactory.Object);
        }

        [Test]
        public void CreateDefaultMessagingScenarioFactoryThrowsWhenNoConfigAndNoFallback()
        {
            ResetFactory();
            ResetConfig();

            MessagingScenarioFactory.SetFallback(null);

            try
            {
                CallPrivateCreateDefault();
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.Should().BeOfType<InvalidOperationException>();
                ex.InnerException.Message.Should()
                    .Be("MessagingScenarioFactory.Current has no value. The value can be set via config or by calling the SetCurrent method.");
            }
        }

        [Test]
        public void MessagingScenarioFactoryPassThroughOnCreateQueueConsumer()
        {
            var mockReceiver = new Mock<IReceiver>();
            var mockFactory = new Mock<IMessagingScenarioFactory>();
            mockFactory.Setup(mf => mf.CreateQueueConsumer(It.IsAny<string>())).Returns(mockReceiver.Object);

            ResetFactory();
            MessagingScenarioFactory.SetCurrent(mockFactory.Object);

            MessagingScenarioFactory.CreateQueueConsumer("test").Should().BeSameAs(mockReceiver.Object);
        }

        [Test]
        public void MessagingScenarioFactoryPassThroughOnCreateQueueProducer()
        {
            var mockReceiver = new Mock<ISender>();
            var mockFactory = new Mock<IMessagingScenarioFactory>();
            mockFactory.Setup(mf => mf.CreateQueueProducer(It.IsAny<string>())).Returns(mockReceiver.Object);

            ResetFactory();
            MessagingScenarioFactory.SetCurrent(mockFactory.Object);

            MessagingScenarioFactory.CreateQueueProducer("test").Should().BeSameAs(mockReceiver.Object);
        }

        [Test]
        public void MessagingScenarioFactoryPassThroughOnCreateTopicSubscriber()
        {
            var mockReceiver = new Mock<IReceiver>();
            var mockFactory = new Mock<IMessagingScenarioFactory>();
            mockFactory.Setup(mf => mf.CreateTopicSubscriber(It.IsAny<string>())).Returns(mockReceiver.Object);

            ResetFactory();
            MessagingScenarioFactory.SetCurrent(mockFactory.Object);

            MessagingScenarioFactory.CreateTopicSubscriber("test").Should().BeSameAs(mockReceiver.Object);
        }

        [Test]
        public void MessagingScenarioFactoryPassThroughOnCreateTopicPublisher()
        {
            var mockReceiver = new Mock<ISender>();
            var mockFactory = new Mock<IMessagingScenarioFactory>();
            mockFactory.Setup(mf => mf.CreateTopicPublisher(It.IsAny<string>())).Returns(mockReceiver.Object);

            ResetFactory();
            MessagingScenarioFactory.SetCurrent(mockFactory.Object);

            MessagingScenarioFactory.CreateTopicPublisher("test").Should().BeSameAs(mockReceiver.Object);
        }

        private static void ResetConfig()
        {
            var rootField = typeof(Config).GetField("_root", BindingFlags.NonPublic | BindingFlags.Static);
            var root = (Semimutable<IConfigurationRoot>)rootField.GetValue(null);
            root.GetUnlockValueMethod().Invoke(root, null);
            Config.SetRoot(new Mock<IConfigurationRoot>().Object);
        }

        private static void ResetFactory()
        {
            var factoryField = typeof(MessagingScenarioFactory).GetField("_messagingScenarioFactory", BindingFlags.NonPublic | BindingFlags.Static);
            var factory = (Semimutable<IMessagingScenarioFactory>)factoryField.GetValue(null);
            factory.GetUnlockValueMethod().Invoke(factory, null);
            MessagingScenarioFactory.SetCurrent(null);
        }

        private static IMessagingScenarioFactory CallPrivateCreateDefault()
        {
            var createMethod = typeof(MessagingScenarioFactory).GetMethod("CreateDefaultMessagingScenarioFactory", BindingFlags.NonPublic | BindingFlags.Static);
            return (IMessagingScenarioFactory)createMethod.Invoke(null, null);
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RockLib.Messaging.NamedPipes;
using System.IO;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class MessagingScenarioFactoryTests
    {
        [Test]
        public void CreateSenderCreatesSenderWithSingleSenderConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\SingleSender_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            var sender = (NamedPipeSender)config.CreateSender("Pipe1");

            sender.Name.Should().Be("Pipe1");
            sender.PipeName.Should().Be("PipeName1");
            sender.Compressed.Should().BeTrue();
        }

        [Test]
        public void CreateSenderCreatesSendersWithMultipleSendersConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\MultipleSenders_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            var sender1 = (NamedPipeSender)config.CreateSender("Pipe1");

            sender1.Name.Should().Be("Pipe1");
            sender1.PipeName.Should().Be("PipeName1");
            sender1.Compressed.Should().BeTrue();

            var sender2 = (NamedPipeSender)config.CreateSender("Pipe2");

            sender2.Name.Should().Be("Pipe2");
            sender2.PipeName.Should().Be("PipeName2");
            sender2.Compressed.Should().BeFalse();
        }

        [Test]
        public void CreateReceiverCreatesReceiverWithSingleReceiverConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\SingleReceiver_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            var receiver = (NamedPipeReceiver)config.CreateReceiver("Pipe1");

            receiver.Name.Should().Be("Pipe1");
            receiver.PipeName.Should().Be("PipeName1");
        }

        [Test]
        public void CreateReceiverCreatesReceiversWithMultipleReceiversConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\MultipleReceivers_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            var receiver1 = (NamedPipeReceiver)config.CreateReceiver("Pipe1");

            receiver1.Name.Should().Be("Pipe1");
            receiver1.PipeName.Should().Be("PipeName1");

            var receiver2 = (NamedPipeReceiver)config.CreateReceiver("Pipe2");

            receiver2.Name.Should().Be("Pipe2");
            receiver2.PipeName.Should().Be("PipeName2");
        }
    }
}

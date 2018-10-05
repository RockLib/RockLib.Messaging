using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

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

            var sender = (FakeSender)config.CreateSender("Pipe1");

            sender.Name.Should().Be("Pipe1");
            sender.PipeName.Should().Be("PipeName1");
        }

        [Test]
        public void CreateSenderCreatesSendersWithMultipleSendersConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\MultipleSenders_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            var sender1 = (FakeSender)config.CreateSender("Pipe1");

            sender1.Name.Should().Be("Pipe1");
            sender1.PipeName.Should().Be("PipeName1");

            var sender2 = (FakeSender)config.CreateSender("Pipe2");

            sender2.Name.Should().Be("Pipe2");
            sender2.PipeName.Should().Be("PipeName2");
        }

        [Test]
        public void CreateReceiverCreatesReceiverWithSingleReceiverConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\SingleReceiver_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            var receiver = (FakeReceiver)config.CreateReceiver("Pipe1");

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

            var receiver1 = (FakeReceiver)config.CreateReceiver("Pipe1");

            receiver1.Name.Should().Be("Pipe1");
            receiver1.PipeName.Should().Be("PipeName1");

            var receiver2 = (FakeReceiver)config.CreateReceiver("Pipe2");

            receiver2.Name.Should().Be("Pipe2");
            receiver2.PipeName.Should().Be("PipeName2");
        }

        [Test]
        public void CreateSenderCreatesSenderUsingDefaultSenderType()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\DefaultSender_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            var sender = (FakeSender)config.CreateSender("Pipe1");

            sender.Name.Should().Be("Pipe1");
            sender.PipeName.Should().Be("PipeName1");
        }

        [Test]
        public void CreateReceiverCreatesReceiverUsingDefaultReceiverType()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"CustomConfigFiles\DefaultReceiver_appsettings.json", false)
                .Build()
                .GetSection("RockLib.Messaging");

            var receiver = (FakeReceiver)config.CreateReceiver("Pipe1");

            receiver.Name.Should().Be("Pipe1");
            receiver.PipeName.Should().Be("PipeName1");
        }
    }

    public class FakeSender : ISender
    {
        public string Name { get; set; }
        public string PipeName { get; set; }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Task SendAsync(SenderMessage message)
        {
            throw new System.NotImplementedException();
        }
    }

    public class FakeReceiver : IReceiver
    {
        public string Name { get; set; }
        public string PipeName { get; set; }
        public IMessageHandler MessageHandler { get; set; }

        public event EventHandler Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

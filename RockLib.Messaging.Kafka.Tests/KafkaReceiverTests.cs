using FluentAssertions;
using Xunit;
using System;
using Confluent.Kafka;
using Moq;
using RockLib.Dynamic;

namespace RockLib.Messaging.Kafka.Tests
{
    public class KafkaReceiverTests
    {
        [Fact(DisplayName = "KafkaReceiver constructor sets appropriate properties")]
        public void KafkaReceiverConstructorHappyPath1()
        {
            var name = "name";
            var topic = "topic";
            var groupId = "groupId";
            var servers = "bootstrapServers";
            var config = new ConsumerConfig();
            var sender = new KafkaReceiver(name, topic, groupId, servers, config);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.Config.Should().BeSameAs(config);
            sender.Config.GroupId.Should().Be(groupId);
            sender.Config.BootstrapServers.Should().Be(servers);
        }

        [Fact(DisplayName = "KafkaReceiver constructor throws on null topic")]
        public void KafkaReceiverConstructorSadPath1()
        {
            Action action = () => new KafkaReceiver("name", null, "groupId", "servers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver constructor throws on null groupId")]
        public void KafkaReceiverConstructorSadPath2()
        {
            Action action = () => new KafkaReceiver("name", "topic", null, "servers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver constructor throws on null bootstrapServers")]
        public void KafkaReceiverConstructorSadPath3()
        {
            Action action = () => new KafkaReceiver("name", "topic", "groupId", null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver Start starts the receiver and tracking threads")]
        public void KafkaReceiverStartHappyPath()
        {
            var receiver = new KafkaReceiver("name", "one_topic", "groupId", "servers");

            var consumerMock = new Mock<IConsumer<Ignore, string>>();
            consumerMock.Setup(c => c.Subscribe(It.IsAny<string>()));

            var unlockedReceiver = receiver.Unlock();
            unlockedReceiver._consumer = new Lazy<IConsumer<Ignore, string>>(() => consumerMock.Object);
            unlockedReceiver.Start();

            consumerMock.Verify(cm => cm.Subscribe("one_topic"), Times.Once);

            unlockedReceiver.Dispose();
        }

        [Fact(DisplayName = "KafkaReceiver Dispose stops the receiver and tracking threads")]
        public void KafkaReceiverDisposeHappyPath()
        {
            var receiver = new KafkaReceiver("name", "one_topic", "groupId", "servers");

            var consumerMock = new Mock<IConsumer<Ignore, string>>();
            consumerMock.Setup(cm => cm.Close());
            consumerMock.Setup(cm => cm.Dispose());

            var unlockedReceiver = receiver.Unlock();
            unlockedReceiver._consumer = new Lazy<IConsumer<Ignore, string>>(() => consumerMock.Object);
            unlockedReceiver.Start();

            unlockedReceiver.Dispose();

            consumerMock.Verify(cm => cm.Close(), Times.Once);
            consumerMock.Verify(cm => cm.Dispose(), Times.Once);
        }
    }
}

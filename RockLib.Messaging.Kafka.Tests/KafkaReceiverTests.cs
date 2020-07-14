using FluentAssertions;
using Xunit;
using System;
using Confluent.Kafka;
using Moq;
using RockLib.Dynamic;
using System.Threading;

namespace RockLib.Messaging.Kafka.Tests
{
    public class KafkaReceiverTests
    {
        [Fact(DisplayName = "KafkaReceiver constructor 1 sets appropriate properties")]
        public void KafkaReceiverConstructor1HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var groupId = "groupId";
            var servers = "bootstrapServers";
            var sender = new KafkaReceiver(name, topic, groupId, servers);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.Consumer.Should().NotBeNull();
        }

        [Fact(DisplayName = "KafkaReceiver constructor 2 sets appropriate properties")]
        public void KafkaReceiverConstructor2HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var consumer = new Mock<IConsumer<Ignore, string>>().Object;
            var sender = new KafkaReceiver(name, topic, consumer);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.Consumer.Should().BeSameAs(consumer);
        }

        [Fact(DisplayName = "KafkaReceiver constructor 1 throws on null topic")]
        public void KafkaReceiverConstructor1SadPath1()
        {
            Action action = () => new KafkaReceiver("name", null, "groupId", "servers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver constructor 1 throws on null groupId")]
        public void KafkaReceiverConstructor1SadPath2()
        {
            Action action = () => new KafkaReceiver("name", "topic", null, "servers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver constructor 1 throws on null bootstrapServers")]
        public void KafkaReceiverConstructor1SadPath3()
        {
            Action action = () => new KafkaReceiver("name", "topic", "groupId", null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver constructor 2 throws on null topic")]
        public void KafkaReceiverConstructor2SadPath1()
        {
            Action action = () => new KafkaReceiver("name", null, new Mock<IConsumer<Ignore, string>>().Object);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver constructor 2 throws on null consumer")]
        public void KafkaReceiverConstructor2SadPath2()
        {
            Action action = () => new KafkaReceiver("name", "topic", null);
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

        [Fact(DisplayName = "KafkaReceiver receives message from consumer")]
        public void KafkaReceiverHappyPath()
        {
            var message = new Message<Ignore, string>() { Value = "This is the expected message!" };
            var result = new ConsumeResult<Ignore, string>() { Message = message };

            var consumerMock = new Mock<IConsumer<Ignore, string>>();
            consumerMock.Setup(c => c.Subscribe(It.IsAny<string>()));
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>())).Returns(result);

            var waitHandle = new AutoResetEvent(false);

            string receivedMessage = null;

            using (var receiver = new KafkaReceiver("NAME", "TOPIC", "GROUPID", "SERVER"))
            {
                var unlockedReceiver = receiver.Unlock();
                unlockedReceiver._consumer = new Lazy<IConsumer<Ignore, string>>(() => consumerMock.Object);

                receiver.Start(async m =>
                {
                    receivedMessage = m.StringPayload;
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            consumerMock.Verify(m => m.Consume(It.IsAny<CancellationToken>()));

            receivedMessage.Should().Be("This is the expected message!");
        }
    }
}

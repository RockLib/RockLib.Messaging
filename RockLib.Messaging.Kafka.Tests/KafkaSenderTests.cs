using FluentAssertions;
using Xunit;
using System;
using Confluent.Kafka;
using RockLib.Dynamic;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace RockLib.Messaging.Kafka.Tests
{
    public class KafkaSenderTests
    {
        [Fact(DisplayName = "KafkaSender constructor 1 sets appropriate properties")]
        public void KafkaSenderConstructor1HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var servers = "bootstrapServers";
            var timeout = 100;
            var sender = new KafkaSender(name, topic, servers, timeout);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.Producer.Should().NotBeNull();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 sets appropriate properties")]
        public void KafkaSenderConstructor2HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var producer = new Mock<IProducer<Null, string>>().Object;
            var sender = new KafkaSender(name, topic, producer);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.Producer.Should().BeSameAs(producer);
        }

        [Fact(DisplayName = "KafkaSender constructor throws on null name")]
        public void KafkaSenderConstructorSadPath1()
        {
            Action action = () => new KafkaSender(null, "topic", "boostrapServers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor throws on null topic")]
        public void KafkaSenderConstructorSadPath2()
        {
            Action action = () => new KafkaSender("name", null, "boostrapServers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor throws on null bootstrapServers")]
        public void KafkaSenderConstructorSadPath3()
        {
            Action action = () => new KafkaSender("name", "topic", null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 throws on null name")]
        public void KafkaSenderConstructor2SadPath1()
        {
            Action action = () => new KafkaSender(null, "topic", new Mock<IProducer<Null, string>>().Object);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 throws on null topic")]
        public void KafkaSenderConstructor2SadPath2()
        {
            Action action = () => new KafkaSender("name", null, new Mock<IProducer<Null, string>>().Object);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 throws on null producer")]
        public void KafkaSenderConstructor2SadPath3()
        {
            Action action = () => new KafkaSender("name", "topic", null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender SendAsync sets OriginatingSystem to Kafka and sets message")]
        public async Task KafkaSenderSendAsyncHappyPath()
        {
            var message = "This is a message";
            var producerMock = new Mock<IProducer<Null, string>>();
            producerMock
                .Setup(pm => pm.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<Null, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeliveryResult<Null, string>)null);

            var sender = new KafkaSender("name", "topic", "servers");
            sender.Unlock()._producer = new Lazy<IProducer<Null, string>>(() => producerMock.Object);

            await sender.SendAsync(new SenderMessage(message));

            producerMock.Verify(pm => pm.ProduceAsync("topic", 
                It.Is<Message<Null, string>>(m => m.Value == message && Encoding.UTF8.GetString(m.Headers[1].GetValueBytes()) == "Kafka"), 
                It.IsAny<CancellationToken>()));
        }

        [Fact(DisplayName = "KafkaSender Dispose calls Flush and Dispose on producer")]
        public void KafkaSenderDispose()
        {
            var producerMock = new Mock<IProducer<Null, string>>();
            producerMock.Setup(pm => pm.Flush(It.IsAny<TimeSpan>()));
            producerMock.Setup(pm => pm.Dispose());

            var senderUnlocked = new KafkaSender("name", "topic", "servers").Unlock();
            senderUnlocked._producer = new Lazy<IProducer<Null, string>>(() => producerMock.Object);
            _ = senderUnlocked._producer.Value; 

            senderUnlocked.Dispose();

            producerMock.Verify(pm => pm.Flush(It.IsAny<TimeSpan>()), Times.Once);
            producerMock.Verify(pm => pm.Dispose(), Times.Once);
        }
    }
}

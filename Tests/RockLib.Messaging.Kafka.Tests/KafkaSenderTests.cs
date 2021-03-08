using FluentAssertions;
using Xunit;
using System;
using System.Net;
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
            sender.BootstrapServers.Should().Be(servers);
            sender.MessageTimeoutMs.Should().Be(timeout);
            sender.Producer.Should().NotBeNull();
            sender.SchemaId.Should().BeNull();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 sets appropriate properties")]
        public void KafkaSenderConstructor2HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var servers = "bootstrapServers";
            var timeout = 100;
            var config = new ProducerConfig()
            {
                BootstrapServers = servers,
                MessageTimeoutMs = timeout
            };
            var sender = new KafkaSender(name, topic, config);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.BootstrapServers.Should().Be(servers);
            sender.MessageTimeoutMs.Should().Be(timeout);
            sender.Producer.Should().NotBeNull();
            sender.SchemaId.Should().BeNull();
        }
        
        [Fact(DisplayName = "KafkaSender constructor 3 sets appropriate properties")]
        public void KafkaSenderConstructor3HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var servers = "bootstrapServers";
            var timeout = 100;
            var schemaId = 10;
            var sender = new KafkaSender(name, topic, schemaId, servers, timeout);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.BootstrapServers.Should().Be(servers);
            sender.MessageTimeoutMs.Should().Be(timeout);
            sender.Producer.Should().NotBeNull();
            sender.SchemaId.Should().Be(schemaId);
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
            Action action = () => new KafkaSender(null, "topic", new ProducerConfig());
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 throws on null topic")]
        public void KafkaSenderConstructor2SadPath2()
        {
            Action action = () => new KafkaSender("name", null, new ProducerConfig());
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 throws on null producer config")]
        public void KafkaSenderConstructor2SadPath3()
        {
            Action action = () => new KafkaSender("name", "topic", null);
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact(DisplayName = "KafkaSender constructor 3 throws on null name")]
        public void KafkaSenderConstructor3SadPath1()
        {
            Action action = () => new KafkaSender(null, "topic", 10, "boostrapServers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 3 throws on null topic")]
        public void KafkaSenderConstructor3SadPath2()
        {
            Action action = () => new KafkaSender("name", null, 10, "boostrapServers");
            action.Should().Throw<ArgumentNullException>();
        }

        
        [Fact(DisplayName = "KafkaSender constructor 3 throws on negative schemaId")]
        public void KafkaSenderConstructor3SadPath4()
        {
            Action action = () => new KafkaSender("name", "topic", -1, "bootstrapServers");
            action.Should().Throw<ArgumentOutOfRangeException>();
        }
        
        [Fact(DisplayName = "KafkaSender constructor 3 throws on null bootstrapServers")]
        public void KafkaSenderConstructor3SadPath3()
        {
            Action action = () => new KafkaSender("name", "topic", 10, null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender SendAsync sets OriginatingSystem to Kafka and sets message")]
        public async Task KafkaSenderSendAsyncHappyPath1()
        {
            var message = "This is a message";
            var producerMock = new Mock<IProducer<string, byte[]>>();
            producerMock
                .Setup(pm => pm.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeliveryResult<string, byte[]>)null);

            var sender = new KafkaSender("name", "topic", "servers");
            sender.Unlock()._producer = new Lazy<IProducer<string, byte[]>>(() => producerMock.Object);

            await sender.SendAsync(new SenderMessage(message));

            producerMock.Verify(pm => pm.ProduceAsync("topic", 
                It.Is<Message<string, byte[]>>(m => Encoding.UTF8.GetString(m.Value) == message && Encoding.UTF8.GetString(m.Headers[1].GetValueBytes()) == "Kafka"), 
                It.IsAny<CancellationToken>()));
        }
        
        [Fact(DisplayName = "KafkaSender SendAsync adds schema ID to message payload when available")]
        public async Task KafkaSenderSendAsyncHappyPath2()
        {
            var message = "This is a message";
            var schemaId = 100;
            var producerMock = new Mock<IProducer<string, byte[]>>();
            Message<string, byte[]> sentMessage = null;
            producerMock
                .Setup(pm => pm.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()))
                .Callback<string, Message<string, byte[]>, CancellationToken>((str, msg, ct) => sentMessage = msg)
                .ReturnsAsync((DeliveryResult<string, byte[]>)null);

            var sender = new KafkaSender("name", "topic", schemaId, "servers");
            sender.Unlock()._producer = new Lazy<IProducer<string, byte[]>>(() => producerMock.Object);

            await sender.SendAsync(new SenderMessage(message));
            
            Assert.Equal(0, BitConverter.ToInt32(sentMessage.Value, 0));
            Assert.Equal(schemaId, IPAddress.NetworkToHostOrder(BitConverter.ToInt32(sentMessage.Value, 1)));
            
            var memory = new Memory<byte>(sentMessage.Value, 5, sentMessage.Value.Length - 5);
            Assert.Equal(message, Encoding.UTF8.GetString(memory.ToArray()));
            Assert.Equal("Kafka", Encoding.UTF8.GetString(sentMessage.Headers[1].GetValueBytes()));
        }

        [Fact(DisplayName = "KafkaSender Dispose calls Flush and Dispose on producer")]
        public void KafkaSenderDispose()
        {
            var producerMock = new Mock<IProducer<string, byte[]>>();
            producerMock.Setup(pm => pm.Flush(It.IsAny<TimeSpan>()));
            producerMock.Setup(pm => pm.Dispose());

            var senderUnlocked = new KafkaSender("name", "topic", "servers").Unlock();
            senderUnlocked._producer = new Lazy<IProducer<string, byte[]>>(() => producerMock.Object);
            _ = senderUnlocked._producer.Value; 

            senderUnlocked.Dispose();

            producerMock.Verify(pm => pm.Flush(It.IsAny<TimeSpan>()), Times.Once);
            producerMock.Verify(pm => pm.Dispose(), Times.Once);
        }
    }
}

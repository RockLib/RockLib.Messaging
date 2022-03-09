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
    public static class KafkaSenderTests
    {
        [Fact(DisplayName = "KafkaSender constructor 1 sets appropriate properties")]
        public static void KafkaSenderConstructor1HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var servers = "bootstrapServers";
            var timeout = 100;
            using var sender = new KafkaSender(name, topic, servers, timeout);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.BootstrapServers.Should().Be(servers);
            sender.MessageTimeoutMs.Should().Be(timeout);
            sender.Producer.Should().NotBeNull();
            sender.SchemaId.Should().Be(0);
        }

        [Fact(DisplayName = "KafkaSender constructor 2 sets appropriate properties")]
        public static void KafkaSenderConstructor2HappyPath()
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
            using var sender = new KafkaSender(name, topic, config);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.BootstrapServers.Should().Be(servers);
            sender.MessageTimeoutMs.Should().Be(timeout);
            sender.Producer.Should().NotBeNull();
            sender.SchemaId.Should().Be(0);
        }
        
        [Fact(DisplayName = "KafkaSender constructor 3 sets appropriate properties")]
        public static void KafkaSenderConstructor3HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var servers = "bootstrapServers";
            var timeout = 100;
            var schemaId = 10;
            using var sender = new KafkaSender(name, topic, schemaId, servers, timeout);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.BootstrapServers.Should().Be(servers);
            sender.MessageTimeoutMs.Should().Be(timeout);
            sender.Producer.Should().NotBeNull();
            sender.SchemaId.Should().Be(schemaId);
        }
        
        [Fact(DisplayName = "KafkaSender constructor 4 sets appropriate properties")]
        public static void KafkaSenderConstructor4HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var servers = "bootstrapServers";
            var timeout = 100;
            var schemaId = 10;
            var config = new ProducerConfig()
            {
                BootstrapServers = servers,
                MessageTimeoutMs = timeout
            };
            using var sender = new KafkaSender(name, topic, schemaId, config);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.BootstrapServers.Should().Be(servers);
            sender.MessageTimeoutMs.Should().Be(timeout);
            sender.Producer.Should().NotBeNull();
            sender.SchemaId.Should().Be(schemaId);
        }

        [Fact(DisplayName = "KafkaSender constructor throws on null name")]
        public static void KafkaSenderConstructorSadPath1()
        {
            Func<KafkaSender> action = () => new KafkaSender(null!, "topic", "boostrapServers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor throws on null topic")]
        public static void KafkaSenderConstructorSadPath2()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", null!, "boostrapServers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor throws on null bootstrapServers")]
        public static void KafkaSenderConstructorSadPath3()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", "topic", null!);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 throws on null name")]
        public static void KafkaSenderConstructor2SadPath1()
        {
            Func<KafkaSender> action = () => new KafkaSender(null!, "topic", new ProducerConfig());
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 throws on null topic")]
        public static void KafkaSenderConstructor2SadPath2()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", null!, new ProducerConfig());
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 2 throws on null producer config")]
        public static void KafkaSenderConstructor2SadPath3()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", "topic", null!);
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact(DisplayName = "KafkaSender constructor 3 throws on null name")]
        public static void KafkaSenderConstructor3SadPath1()
        {
            Func<KafkaSender> action = () => new KafkaSender(null!, "topic", 10, "boostrapServers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 3 throws on null topic")]
        public static void KafkaSenderConstructor3SadPath2()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", null!, 10, "boostrapServers");
            action.Should().Throw<ArgumentNullException>();
        }

        
        [Fact(DisplayName = "KafkaSender constructor 3 throws on negative schemaId")]
        public static void KafkaSenderConstructor3SadPath4()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", "topic", -1, "bootstrapServers");
            action.Should().Throw<ArgumentOutOfRangeException>();
        }
        
        [Fact(DisplayName = "KafkaSender constructor 3 throws on null bootstrapServers")]
        public static void KafkaSenderConstructor3SadPath3()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", "topic", 10, null!);
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact(DisplayName = "KafkaSender constructor 4 throws on null name")]
        public static void KafkaSenderConstructor4SadPath1()
        {
            Func<KafkaSender> action = () => new KafkaSender(null!, "topic", 1, new ProducerConfig());
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 4 throws on null topic")]
        public static void KafkaSenderConstructor4SadPath2()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", null!, 1, new ProducerConfig());
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact(DisplayName = "KafkaSender constructor 4 throws on invalid schema ID")]
        public static void KafkaSenderConstructor4SadPath3()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", "topic", 0, new ProducerConfig());
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact(DisplayName = "KafkaSender constructor 4 throws on null producer config")]
        public static void KafkaSenderConstructor4SadPath4()
        {
            Func<KafkaSender> action = () => new KafkaSender("name", "topic", 1, null!);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaSender SendAsync sets OriginatingSystem to Kafka and sets message")]
        public static async Task KafkaSenderSendAsyncHappyPath1()
        {
            var message = "This is a message";
            var producerMock = new Mock<IProducer<string, byte[]>>();
            producerMock
                .Setup(pm => pm.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DeliveryResult<string, byte[]>)null!);

            using var sender = new KafkaSender("name", "topic", "servers");
            sender.Unlock()._producer = new Lazy<IProducer<string, byte[]>>(() => producerMock.Object);

            await sender.SendAsync(new SenderMessage(message)).ConfigureAwait(false);

            producerMock.Verify(pm => pm.ProduceAsync("topic", 
                It.Is<Message<string, byte[]>>(m => Encoding.UTF8.GetString(m.Value) == message && Encoding.UTF8.GetString(m.Headers[1].GetValueBytes()) == "Kafka"), 
                It.IsAny<CancellationToken>()));
        }
        
        [Fact(DisplayName = "KafkaSender SendAsync adds schema ID to message payload when available")]
        public static async Task KafkaSenderSendAsyncHappyPath2()
        {
            var message = "This is a message";
            var schemaId = 100;
            var producerMock = new Mock<IProducer<string, byte[]>>();
            Message<string, byte[]>? sentMessage = null;
            producerMock
                .Setup(pm => pm.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()))
                .Callback<string, Message<string, byte[]>, CancellationToken>((str, msg, ct) => sentMessage = msg)
                .ReturnsAsync((DeliveryResult<string, byte[]>)null!);

            using var sender = new KafkaSender("name", "topic", schemaId, "servers");
            sender.Unlock()._producer = new Lazy<IProducer<string, byte[]>>(() => producerMock.Object);

            await sender.SendAsync(new SenderMessage(message)).ConfigureAwait(false);
            
            Assert.Equal(0, BitConverter.ToInt32(sentMessage!.Value, 0));
            Assert.Equal(schemaId, IPAddress.NetworkToHostOrder(BitConverter.ToInt32(sentMessage.Value, 1)));
            
            var memory = new Memory<byte>(sentMessage.Value, 5, sentMessage.Value.Length - 5);
            Assert.Equal(message, Encoding.UTF8.GetString(memory.ToArray()));
            Assert.Equal("Kafka", Encoding.UTF8.GetString(sentMessage.Headers[1].GetValueBytes()));
        }

        [Fact(DisplayName = "KafkaSender Dispose calls Flush and Dispose on producer")]
        public static void KafkaSenderDispose()
        {
            var producerMock = new Mock<IProducer<string, byte[]>>();
            producerMock.Setup(pm => pm.Flush(It.IsAny<TimeSpan>()));
            producerMock.Setup(pm => pm.Dispose());

            using var sender = new KafkaSender("name", "topic", "servers");
            var senderUnlocked = sender.Unlock();
            senderUnlocked._producer = new Lazy<IProducer<string, byte[]>>(() => producerMock.Object);
            _ = senderUnlocked._producer.Value; 

            senderUnlocked.Dispose();

            producerMock.Verify(pm => pm.Flush(It.IsAny<TimeSpan>()), Times.Once);
            producerMock.Verify(pm => pm.Dispose(), Times.Once);
        }
    }
}

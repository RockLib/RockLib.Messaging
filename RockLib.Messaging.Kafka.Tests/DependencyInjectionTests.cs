using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RockLib.Messaging.Kafka.DependencyInjection;
using Confluent.Kafka;

namespace RockLib.Messaging.Kafka.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void KafkaSenderTest()
        {
            var services = new ServiceCollection();

            services.AddKafkaSender("mySender", options =>
            {
                options.Topic= "SenderTopic";
                options.BootstrapServers = "SenderServer";
                options.MessageTimeoutMs = 555;
            });

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var kafkaSender = sender.Should().BeOfType<KafkaSender>().Subject;

            kafkaSender.Name.Should().Be("mySender");
            kafkaSender.Topic.Should().Be("SenderTopic");
            kafkaSender.Producer.Should().NotBeNull();
        }

        [Fact]
        public void KafkaReceiverTest()
        {
            var services = new ServiceCollection();

            services.AddKafkaReceiver("myReceiver", options =>
            {
                options.Topic = "ReceiverTopic";
                options.BootstrapServers = "ReceiverServer";
                options.GroupId = "ReceiverGroupId";
            });

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            var kafkaReceiver = receiver.Should().BeOfType<KafkaReceiver>().Subject;

            kafkaReceiver.Name.Should().Be("myReceiver");
            kafkaReceiver.Topic.Should().Be("ReceiverTopic");
            kafkaReceiver.Consumer.Should().NotBeNull();
        }
    }
}

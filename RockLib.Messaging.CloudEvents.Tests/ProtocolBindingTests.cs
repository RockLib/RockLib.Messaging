using FluentAssertions;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class ProtocolBindingTests
    {
        [Fact]
        public void DefaultFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Default.GetHeaderName(attributeName);

            headerName.Should().BeSameAs(attributeName);
        }

        [Fact]
        public void AmqpFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Amqp.GetHeaderName(attributeName);

            headerName.Should().Be("cloudEvents:" + attributeName);
        }

        [Fact]
        public void HttpFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Http.GetHeaderName(attributeName);

            headerName.Should().Be("ce_" + attributeName);
        }

        [Fact]
        public void KafkaFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Kafka.GetHeaderName(attributeName);

            headerName.Should().Be("ce_" + attributeName);
        }

        [Fact]
        public void MqttFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Mqtt.GetHeaderName(attributeName);

            headerName.Should().BeSameAs(attributeName);
        }
    }
}

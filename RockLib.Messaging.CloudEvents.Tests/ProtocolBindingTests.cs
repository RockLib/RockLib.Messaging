using FluentAssertions;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class ProtocolBindingTests
    {
        [Fact(DisplayName = "Default field's GetHeaderName method returns attribute name unmodified")]
        public void DefaultFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Default.GetHeaderName(attributeName);

            headerName.Should().BeSameAs(attributeName);
        }

        [Fact(DisplayName = "Amqp field's GetHeaderName method returns 'cloudEvents:' + attribute name")]
        public void AmqpFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Amqp.GetHeaderName(attributeName);

            headerName.Should().Be("cloudEvents:" + attributeName);
        }

        [Fact(DisplayName = "Http field's GetHeaderName method returns 'ce_' + attribute name")]
        public void HttpFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Http.GetHeaderName(attributeName);

            headerName.Should().Be("ce_" + attributeName);
        }

        [Fact(DisplayName = "Kafka field's GetHeaderName method returns 'ce_' + attribute name")]
        public void KafkaFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Kafka.GetHeaderName(attributeName);

            headerName.Should().Be("ce_" + attributeName);
        }

        [Fact(DisplayName = "Mqtt field's GetHeaderName method returns attribute name unmodified")]
        public void MqttFieldHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBinding.Mqtt.GetHeaderName(attributeName);

            headerName.Should().BeSameAs(attributeName);
        }
    }
}

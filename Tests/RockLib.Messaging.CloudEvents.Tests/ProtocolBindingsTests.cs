using FluentAssertions;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class ProtocolBindingsTests
    {
        [Fact(DisplayName = "Default field's GetHeaderName method returns attribute name unmodified")]
        public void DefaultProtocolBindingGetHeaderNameMethodHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBindings.Default.GetHeaderName(attributeName);

            headerName.Should().BeSameAs(attributeName);
        }

        [Fact(DisplayName = "Default field's GetAttributeName method returns header name unmodified")]
        public void DefaultProtocolBindingGetAttributeNameMethodHappyPath()
        {
            var headerName = "MyHeader";

            var attributeName = ProtocolBindings.Default.GetAttributeName(headerName, out bool isCloudEventAttribute);

            attributeName.Should().BeSameAs(headerName);
            isCloudEventAttribute.Should().BeTrue();
        }

        [Fact(DisplayName = "Default field's Bind method 1 does nothing")]
        public void DefaultProtocolBindingBindMethod1HappyPath()
        {
            var cloudEvent = new CloudEvent
            {
                Source = "MySource",
                Type = "MyType",
                Attributes = { ["foo"] = "abc" },
                Headers = { ["bar"] = "xyz" }
            };
            var senderMessage = new SenderMessage("");

            senderMessage.Headers.Should().HaveCount(1);
            senderMessage.Headers.Should().ContainKey(HeaderNames.MessageId);

            ProtocolBindings.Default.Bind(cloudEvent, senderMessage);

            senderMessage.Headers.Should().HaveCount(1);
            senderMessage.Headers.Should().ContainKey(HeaderNames.MessageId);
        }

        [Fact(DisplayName = "Default field's Bind method 2 does nothing")]
        public void DefaultProtocolBindingBindMethod2HappyPath()
        {
            using var receiverMessage = new FakeReceiverMessage("")
            {
                Headers =
                {
                    ["source"] = "MySource",
                    ["type"] = "MyType",
                    ["foo"] = "abc",
                    ["bar"] = "xyz",
                }
            };
            var cloudEvent = new CloudEvent();

            cloudEvent.Attributes.Should().BeEmpty();
            cloudEvent.Headers.Should().BeEmpty();

            ProtocolBindings.Default.Bind(receiverMessage, cloudEvent);

            cloudEvent.Attributes.Should().BeEmpty();
            cloudEvent.Headers.Should().BeEmpty();
        }

        [Fact(DisplayName = "Kafka field's GetHeaderName method returns 'ce_' + attribute name")]
        public void KafkaProtocolBindingGetHeaderNameMethodHappyPath()
        {
            var attributeName = "MyAttribute";

            var headerName = ProtocolBindings.Kafka.GetHeaderName(attributeName);

            headerName.Should().Be("ce_" + attributeName);
        }

        [Fact(DisplayName = "Kafka field's GetAttributeName method strips 'ce_' prefix from the header name")]
        public void KafkaProtocolBindingGetAttributeNameMethodHappyPath1()
        {
            var headerName = "ce_MyHeader";

            var attributeName = ProtocolBindings.Kafka.GetAttributeName(headerName, out bool isCloudEventAttribute);

            attributeName.Should().Be("MyHeader");
            isCloudEventAttribute.Should().BeTrue();
        }

        [Fact(DisplayName = "Kafka field's GetAttributeName method returns header name unmodified when not prefixed with 'ce_'")]
        public void KafkaProtocolBindingGetAttributeNameMethodHappyPath2()
        {
            var headerName = "AnotherHeader";

            var attributeName = ProtocolBindings.Kafka.GetAttributeName(headerName, out bool isCloudEventAttribute);

            attributeName.Should().Be(headerName);
            isCloudEventAttribute.Should().BeFalse();
        }

        [Fact(DisplayName = "Kafka field's Bind method 1 remaps 'ce_partitionkey' to 'Kafka.Key'")]
        public void KafkaProtocolBindingBindMethod1HappyPath1()
        {
            var cloudEvent = new CloudEvent
            {
                Attributes = { ["partitionkey"] = "MyPartitionKey" }
            };
            var senderMessage = new SenderMessage("")
            {
                Headers = { ["ce_partitionkey"] = "MyPartitionKey" }
            };

            senderMessage.Headers.Should().HaveCount(2);
            senderMessage.Headers.Should().ContainKey(HeaderNames.MessageId);
            senderMessage.Headers.Should().ContainKey("ce_partitionkey");

            ProtocolBindings.Kafka.Bind(cloudEvent, senderMessage);

            senderMessage.Headers.Should().HaveCount(2);
            senderMessage.Headers.Should().ContainKey(HeaderNames.MessageId);
            senderMessage.Headers.Should().ContainKey("Kafka.Key");
        }

        [Fact(DisplayName = "Kafka field's Bind method 2 remaps 'Kafka.Key' to 'partitionkey'")]
        public void KafkaProtocolBindingBindMethod2HappyPath1()
        {
            using var receiverMessage = new FakeReceiverMessage("")
            {
                Headers = { ["Kafka.Key"] = "MyKafkaKey" }
            };
            var cloudEvent = new CloudEvent
            {
                Attributes = { ["Kafka.Key"] = "MyKafkaKey" }
            };

            cloudEvent.Attributes.Should().HaveCount(1);
            cloudEvent.Attributes.Should().ContainKey("Kafka.Key");
            cloudEvent.Headers.Should().BeEmpty();

            ProtocolBindings.Kafka.Bind(receiverMessage, cloudEvent);

            cloudEvent.Attributes.Should().HaveCount(1);
            cloudEvent.Attributes.Should().ContainKey("partitionkey");
            cloudEvent.Headers.Should().BeEmpty();
        }
    }
}

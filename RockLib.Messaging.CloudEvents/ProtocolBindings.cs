using RockLib.Messaging.CloudEvents.Partitioning;
using System.Text.RegularExpressions;
using static RockLib.Messaging.CloudEvents.ProtocolBindings.Constants;
using static RockLib.Messaging.CloudEvents.PartitionedEvent;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines various protocol bindings.
    /// </summary>
    public static class ProtocolBindings
    {
        /// <summary>
        /// Constants used by protocol binding implementations.
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// The name of the header of a <see cref="SenderMessage"/> or <see cref=
            /// "IReceiverMessage"/> where the Key of a Kafka message is stored.
            /// </summary>
            public const string KafkaKeyHeader = "Kafka.Key";

            /// <summary>
            /// The prefix to apply to the attributes of a <see cref="CloudEvent"/> when converting
            /// to a <see cref="SenderMessage"/>.
            /// </summary>
            public const string KafkaHeaderPrefix = "ce_";
        }

        /// <summary>
        /// The default protocol binding.
        /// </summary>
        public static readonly IProtocolBinding Default = new DefaultProtocolBinding();

        /// <summary>
        /// The protocol binding for Kafka.
        /// </summary>
        public static readonly IProtocolBinding Kafka = new KafkaProtocolBinding();

        private class DefaultProtocolBinding : IProtocolBinding
        {
            public string GetHeaderName(string attributeName) => attributeName;

            public string GetAttributeName(string headerName, out bool isCloudEventAttribute)
            {
                isCloudEventAttribute = true;
                return headerName;
            }

            public void Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage) { }

            public void Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent) { }
        }

        private class KafkaProtocolBinding : IProtocolBinding
        {
            public static readonly Regex AttributeNameRegex = new Regex("^" + KafkaHeaderPrefix);

            public string GetHeaderName(string attributeName) => KafkaHeaderPrefix + attributeName;

            public string GetAttributeName(string headerName, out bool isCloudEventAttribute)
            {
                var attributeName = AttributeNameRegex.Replace(headerName, "");
                isCloudEventAttribute = attributeName != headerName;
                return attributeName;
            }

            public void Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage)
            {
                if (fromCloudEvent.GetPartitionKey() is string kafkaKey)
                {
                    toSenderMessage.Headers.Remove(GetHeaderName(PartitionKeyAttribute));
                    toSenderMessage.Headers[KafkaKeyHeader] = kafkaKey;
                }
            }

            public void Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent)
            {
                if (fromReceiverMessage.Headers.TryGetValue(KafkaKeyHeader, out string kafkaKey))
                {
                    toCloudEvent.Attributes.Remove(KafkaKeyHeader);
                    toCloudEvent.SetPartitionKey(kafkaKey);
                }
            }
        }
    }
}

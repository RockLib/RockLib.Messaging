using RockLib.Messaging.CloudEvents.Partitioning;
using System;
using System.Text.RegularExpressions;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines various protocol bindings.
    /// </summary>
    public static class ProtocolBindings
    {
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
            string IProtocolBinding.GetHeaderName(string attributeName) => attributeName;
            string IProtocolBinding.GetAttributeName(string headerName, out bool isCloudEventAttribute)
            {
                isCloudEventAttribute = true;
                return headerName;
            }
            void IProtocolBinding.Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage) { }
            void IProtocolBinding.Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent) { }
        }

        private class KafkaProtocolBinding : IProtocolBinding
        {
            public const string ContentTypeAttribute = "content-type";
            public const string CloudEventsMediaTypePrefix = "application/cloudevents";
            public const string JsonMediaTypeSuffix = "+json";
            public const string KafkaKeyHeader = "Kafka.Key";
            public const string HeaderPrefix = "ce_";
            public static readonly Regex AttributeNameRegex = new Regex("^" + HeaderPrefix);

            private string GetHeaderName(string attributeName) => HeaderPrefix + attributeName;

            string IProtocolBinding.GetHeaderName(string attributeName) => GetHeaderName(attributeName);

            string IProtocolBinding.GetAttributeName(string headerName, out bool isCloudEventAttribute)
            {
                var attributeName = AttributeNameRegex.Replace(headerName, "");
                isCloudEventAttribute = attributeName != headerName;
                return attributeName;
            }

            void IProtocolBinding.Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage)
            {
                if (fromCloudEvent.GetPartitionKey() is string kafkaKey)
                {
                    toSenderMessage.Headers.Remove(GetHeaderName(PartitionedEvent.PartitionKeyAttribute));
                    toSenderMessage.Headers[KafkaKeyHeader] = kafkaKey;
                }

                if (!string.IsNullOrEmpty(fromCloudEvent.DataContentType))
                {
                    toSenderMessage.Headers.Remove(GetHeaderName(CloudEvent.DataContentTypeAttribute));
                    toSenderMessage.Headers[ContentTypeAttribute] = fromCloudEvent.DataContentType;
                }
            }

            void IProtocolBinding.Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent)
            {
                if (fromReceiverMessage.Headers.TryGetValue(KafkaKeyHeader, out string kafkaKey))
                {
                    toCloudEvent.Attributes.Remove(KafkaKeyHeader);
                    toCloudEvent.SetPartitionKey(kafkaKey);
                }

                var structuredMode = false;

                if (fromReceiverMessage.Headers.TryGetValue(ContentTypeAttribute, out string contentType))
                {
                    if (contentType.StartsWith(CloudEventsMediaTypePrefix, StringComparison.OrdinalIgnoreCase))
                        structuredMode = true;

                    toCloudEvent.Headers.Remove(ContentTypeAttribute);
                    toCloudEvent.DataContentType = contentType;
                }

                if (structuredMode)
                {
                    if (!toCloudEvent.ContentType.MediaType.EndsWith(JsonMediaTypeSuffix))
                        throw new InvalidOperationException($"Unsupported media type. Expected value ending in '{JsonMediaTypeSuffix}', but was '{toCloudEvent.ContentType.MediaType}'.");

                    // TODO: Implement structured mode.
                    throw new NotImplementedException("Structured mode is not implemented.");
                }
            }
        }
    }
}

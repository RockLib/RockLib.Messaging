using RockLib.Messaging.CloudEvents.Partitioning;
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
        /// The protocol binding for Amqp.
        /// </summary>
        public static readonly IProtocolBinding Amqp = new AmqpProtocolBinding();

        /// <summary>
        /// The protocol binding for Http.
        /// </summary>
        public static readonly IProtocolBinding Http = new HttpProtocolBinding();

        /// <summary>
        /// The protocol binding for Kafka.
        /// </summary>
        public static readonly IProtocolBinding Kafka = new KafkaProtocolBinding();

        /// <summary>
        /// The protocol binding for Mqtt.
        /// </summary>
        public static readonly IProtocolBinding Mqtt = new MqttProtocolBinding();

        private class DefaultProtocolBinding : IProtocolBinding
        {
            string IProtocolBinding.GetHeaderName(string attributeName) => attributeName;
            string IProtocolBinding.GetAttributeName(string headerName) => headerName;
            void IProtocolBinding.Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage) { }
            void IProtocolBinding.Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent) { }
        }

        private class AmqpProtocolBinding : IProtocolBinding
        {
            public const string Prefix = "cloudEvents:";
            public static readonly Regex AttributeNameRegex = new Regex("^" + Prefix);

            string IProtocolBinding.GetHeaderName(string attributeName) => Prefix + attributeName;
            string IProtocolBinding.GetAttributeName(string headerName) => AttributeNameRegex.Replace(headerName, "");
            void IProtocolBinding.Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage) { }
            void IProtocolBinding.Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent) { }
        }

        private class HttpProtocolBinding : IProtocolBinding
        {
            public const string Prefix = "ce_";
            public static readonly Regex AttributeNameRegex = new Regex("^" + Prefix);

            string IProtocolBinding.GetHeaderName(string attributeName) => Prefix + attributeName;
            string IProtocolBinding.GetAttributeName(string headerName) => AttributeNameRegex.Replace(headerName, "");
            void IProtocolBinding.Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage) { }
            void IProtocolBinding.Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent) { }
        }

        private class KafkaProtocolBinding : IProtocolBinding
        {
            public const string Prefix = "ce_";
            public static readonly Regex AttributeNameRegex = new Regex("^" + Prefix);

            string IProtocolBinding.GetHeaderName(string attributeName) => Prefix + attributeName;
            string IProtocolBinding.GetAttributeName(string headerName) => AttributeNameRegex.Replace(headerName, "");
            void IProtocolBinding.Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage) { }
            void IProtocolBinding.Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent) { }
        }

        private class MqttProtocolBinding : IProtocolBinding
        {
            public string GetHeaderName(string attributeName) => attributeName;
            public string GetAttributeName(string headerName) => headerName;
            public void Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage) { }
            public void Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent) { }
        }
    }
}

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines various protocol bindings.
    /// </summary>
    public static class ProtocolBinding
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
        }

        private class AmqpProtocolBinding : IProtocolBinding
        {
            string IProtocolBinding.GetHeaderName(string attributeName) => "cloudEvents:" + attributeName;
        }

        private class HttpProtocolBinding : IProtocolBinding
        {
            string IProtocolBinding.GetHeaderName(string attributeName) => "ce_" + attributeName;
        }

        private class KafkaProtocolBinding : IProtocolBinding
        {
            string IProtocolBinding.GetHeaderName(string attributeName) => "ce_" + attributeName;
        }

        private class MqttProtocolBinding : IProtocolBinding
        {
            string IProtocolBinding.GetHeaderName(string attributeName) => attributeName;
        }
    }
}

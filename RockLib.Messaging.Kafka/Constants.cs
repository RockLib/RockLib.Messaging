using Confluent.Kafka;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// Defines constants for the RockLib.Messaging.Kafka package.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The name of the header where a Kafka message's Key is stored.
        /// <para>If a <see cref="SenderMessage"/> has a header with this name and a <see cref=
        /// "KafkaSender"/> sends it, then the Key of the outgoing Kafka <see cref=
        /// "Message{TKey, TValue}"/> is set to that header's value.</para>
        /// <para>When a <see cref="KafkaReceiver"/> receives a <see cref="Message{TKey, TValue}"/>
        /// with a non-empty Key, then that Key is found in the <see cref="IReceiverMessage"/>
        /// header with this name.</para>
        /// </summary>
        public const string KafkaKeyHeader = "Kafka.Key";

        /// <summary>
        /// The name of the header containing the schema ID. 
        /// </summary>
        public const string KafkaSchemaIdHeader = "Kafka.SchemaId";

        /// <summary>
        /// Byte value to indicate the raw payload contains a schema ID. When the first byte of the payload is the
        /// <see cref="SchemaIdLeadingByte"/> the subsequent 4 bytes compose the schema ID value according to the Confluent 
        /// <a href="https://docs.confluent.io/platform/current/schema-registry/serdes-develop/index.html#wire-format">wire format</a>
        /// </summary>
        internal const byte SchemaIdLeadingByte = 0;
    }
}

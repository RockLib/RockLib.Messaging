#if !NET451
using Confluent.Kafka;

namespace RockLib.Messaging.Kafka.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="KafkaReceiver"/>.
    /// </summary>
    public class KafkaReceiverOptions
    {
        /// <summary>
        /// Gets or sets the topic to subscribe to.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the list of brokers as a CSV list of broker host or host:port.
        /// </summary>
        public string BootstrapServers { get; set; }

        /// <summary>
        /// Gets or sets the client group id string. All clients sharing the same group.id belong to the same group.
        /// </summary>
        public string GroupId { get; set; }
    }
}
#endif
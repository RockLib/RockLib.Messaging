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

        /// <summary>
        /// Gets or sets a value indicating whether to automatically store offset of last message
        /// provided to application.
        /// </summary>
        public bool EnableAutoOffsetStore { get; set; } = false;

        /// <summary>
        /// Gets or sets the action to take when there is no initial offset in offset store or the
        /// desired offset is out of range: 'smallest','earliest' - automatically reset the offset
        /// to the smallest offset, 'largest','latest' - automatically reset the offset to the
        /// largest offset, 'error' - trigger an error which is retrieved by consuming messages and
        /// checking 'message->err'.
        /// </summary>
        public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Latest;
    }
}
#endif
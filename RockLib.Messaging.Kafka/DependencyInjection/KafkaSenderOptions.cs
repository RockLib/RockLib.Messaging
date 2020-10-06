#if !NET451
namespace RockLib.Messaging.Kafka.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="KafkaSender"/>.
    /// </summary>
    public class KafkaSenderOptions
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
        /// Gets or sets the local local message timeout. This value is only enforced locally and limits the time
        /// a produced message waits for successful delivery. A time of 0 is infinite. This
        /// is the maximum time librdkafka may use to deliver a message (including retries).
        /// Delivery error occurs when either the retry count or the message timeout are exceeded.
        /// </summary>
        public int MessageTimeoutMs { get; set; } = 10000;
    }
}
#endif
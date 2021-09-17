#if !NET451
using Confluent.Kafka;

namespace RockLib.Messaging.Kafka.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="KafkaSender"/>.
    /// </summary>
    public class KafkaSenderOptions : ProducerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaSenderOptions"/> class.
        /// </summary>
        public KafkaSenderOptions()
            : base()
        {
            MessageTimeoutMs = 10000;
        }

        /// <summary>
        /// Gets or sets the topic to subscribe to.
        /// </summary>
        public string Topic { get; set; }
        
        /// <summary>
        /// Gets or sets the schema ID to validate messages against
        /// </summary>
        public int SchemaId { get; set; }
    }
}
#endif

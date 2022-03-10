﻿using Confluent.Kafka;

namespace RockLib.Messaging.Kafka.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="KafkaReceiver"/>.
    /// </summary>
    public class KafkaReceiverOptions : ConsumerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaReceiverOptions"/> class.
        /// </summary>
        public KafkaReceiverOptions()
            : base()
        {
            EnableAutoOffsetStore = false;
            AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest;
        }

        /// <summary>
        /// Gets or sets the topic to subscribe to.
        /// </summary>
        public string? Topic { get; set; }

        /// <summary>
        /// Gets or sets whether the kafka receiver expects schema information in the payload 
        /// </summary>
        public bool SchemaIdRequired { get; set; }
    }
}
using System;
using Confluent.Kafka;
using RockLib.Configuration.ObjectFactory;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// Extension methods related to <see cref="KafkaReceiver"/> and <see cref="KafkaSender"/> statistics events
    /// </summary>
    public static class StatisticsExtensions
    {
        /// <summary>
        /// Sets a handler for the statistics emitted event of the <see cref="KafkaReceiver"/>.
        /// <para>
        /// After setting the event handler, the handler will be called on intervals specified by the
        /// <see cref="ClientConfig.StatisticsIntervalMs"/> property.
        /// </para> 
        /// </summary>
        /// <param name="receiver">The <see cref="KafkaReceiver"/> to set the event handler on</param>
        /// <param name="statisticsEmittedHandler">The event handler to be called when the underlying Consumer emits statistics.
        /// The statistics is a JSON formatted string as defined here:
        /// <a href="https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md">https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md</a>
        /// </param>
        /// <returns>The same <see cref="IReceiver"/> for method chaining</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> or <paramref name="statisticsEmittedHandler"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="receiver"/> is not a <see cref="KafkaReceiver"/>
        /// </exception>
        public static IReceiver AddStatisticsEmittedHandler(this IReceiver receiver, EventHandler<string> statisticsEmittedHandler)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }
            if (statisticsEmittedHandler is null)
            {
                throw new ArgumentNullException(nameof(statisticsEmittedHandler));
            }

            var kafkaReceiver = (receiver as ConfigReloadingProxy<IReceiver>)?.Object as KafkaReceiver;
            kafkaReceiver ??= receiver as KafkaReceiver;

            if (kafkaReceiver is null)
            {
                throw new ArgumentException($"Receiver should be of type {nameof(KafkaReceiver)} or a proxy class wrapping {nameof(KafkaReceiver)}", nameof(receiver));
            }

            kafkaReceiver.StatisticsEmitted += statisticsEmittedHandler;

            return receiver;
        }

        /// <summary>
        /// Sets a handler for the statistics emitted event of the <see cref="KafkaSender"/>.
        /// <para>
        /// After setting the event handler, the handler will be called on intervals specified by the
        /// <see cref="ClientConfig.StatisticsIntervalMs"/> property.
        /// </para>
        /// </summary>
        /// <param name="sender">The <see cref="KafkaSender"/> to set the event handler on</param>
        /// <param name="statisticsEmittedHandler">The event handler to be called when the underlying Producer emits statistics.
        /// The statistics is a JSON formatted string as defined here:
        /// <a href="https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md">https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md</a>
        /// </param>
        /// <returns>The same <see cref="ISender"/> for method chaining</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="sender"/> or <paramref name="statisticsEmittedHandler"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="sender"/> is not a <see cref="KafkaSender"/>
        /// </exception>
        public static ISender AddStatisticsEmittedHandler(this ISender sender, EventHandler<string> statisticsEmittedHandler)
        {
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }
            if (statisticsEmittedHandler is null)
            {
                throw new ArgumentNullException(nameof(statisticsEmittedHandler));
            }

            var kafkaSender = (sender as ConfigReloadingProxy<ISender>)?.Object as KafkaSender;
            kafkaSender ??= sender as KafkaSender;

            if (kafkaSender is null)
            {
                throw new ArgumentException($"Sender should be of type {nameof(KafkaSender)} or a proxy class wrapping {nameof(KafkaSender)}", nameof(sender));
            }

            kafkaSender.StatisticsEmitted += statisticsEmittedHandler;

            return sender;
        }
    }
}
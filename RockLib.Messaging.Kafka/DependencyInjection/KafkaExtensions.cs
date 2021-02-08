#if !NET451
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RockLib.Messaging.DependencyInjection;
using System;

namespace RockLib.Messaging.Kafka.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering Kafka senders and receivers.
    /// </summary>
    public static class KafkaExtensions
    {
        /// <summary>
        /// Adds a <see cref="KafkaSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="KafkaSenderOptions"/>.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddKafkaSender(this IServiceCollection services, string name, Action<KafkaSenderOptions> configureOptions = null)
        {
            return services.AddSender(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<KafkaSenderOptions>>();
                var options = optionsMonitor?.Get(name) ?? new KafkaSenderOptions();
                configureOptions?.Invoke(options);

                return new KafkaSender(name, options.Topic, options);
            });
        }

        /// <summary>
        /// Adds a <see cref="KafkaReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="KafkaReceiverOptions"/>.</param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddKafkaReceiver(this IServiceCollection services, string name, Action<KafkaReceiverOptions> configureOptions = null)
        {
            return services.AddReceiver(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<KafkaReceiverOptions>>();
                var options = optionsMonitor?.Get(name) ?? new KafkaReceiverOptions();
                configureOptions?.Invoke(options);

                return new KafkaReceiver(name, options.Topic, options, options.SynchronousProcessing);
            });
        }
    }
}
#endif
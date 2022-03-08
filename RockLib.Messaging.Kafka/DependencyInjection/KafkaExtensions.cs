using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="configureOptions">
        /// An optional callback for configuring the <see cref="KafkaSenderOptions"/>. If provided,
        /// invoking this callback is the <em>last</em> step in creating and configuring the
        /// options used to create the sender.
        /// </param>
        /// <param name="reloadOnChange">
        /// Whether to create a kafka sender that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddKafkaSender(this IServiceCollection services, string name,
            Action<KafkaSenderOptions>? configureOptions = null, bool reloadOnChange = true)
        {
            return services.AddSender(name, CreateKafkaSender, configureOptions, reloadOnChange);

            ISender CreateKafkaSender(KafkaSenderOptions options, IServiceProvider serviceProvider) =>
                new KafkaSender(name, options.Topic, options);
        }

        /// <summary>
        /// Adds a <see cref="KafkaReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">
        /// An optional callback for configuring the <see cref="KafkaReceiverOptions"/>. If
        /// provided, invoking this callback is the <em>last</em> step in creating and configuring
        /// the options used to create the receiver.
        /// </param>
        /// <param name="reloadOnChange">
        /// Whether to create a kafka receiver that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddKafkaReceiver(this IServiceCollection services, string name,
            Action<KafkaReceiverOptions>? configureOptions = null, bool reloadOnChange = true)
        {
            return services.AddReceiver(name, CreateKafkaReceiver, configureOptions, reloadOnChange);

            IReceiver CreateKafkaReceiver(KafkaReceiverOptions options, IServiceProvider serviceProvider) =>
                new KafkaReceiver(name, options.Topic, options, options.SynchronousProcessing, options.SchemaIdRequired);
        }
    }
}
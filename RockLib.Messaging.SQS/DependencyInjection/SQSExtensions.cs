using Amazon;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using RockLib.Messaging.SQS;
using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering SQS senders and receivers.
    /// </summary>
    public static class SQSExtensions
    {
        /// <summary>The default lifetime of messaging services.</summary>
        private const ServiceLifetime _defaultLifetime = ServiceLifetime.Singleton;

        /// <summary>
        /// Adds an <see cref="SQSSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSSenderOptions"/>.</param>
        /// <param name="reloadOnChange">
        /// Whether to create an SQS sender that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the receiver.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddSQSSender(this IServiceCollection services, string name,
            Action<SQSSenderOptions>? configureOptions = null, bool reloadOnChange = true, ServiceLifetime lifetime = _defaultLifetime)
        {
            return services.AddSender(name, CreateSQSSender, configureOptions, reloadOnChange, lifetime);

            ISender CreateSQSSender(SQSSenderOptions options, IServiceProvider serviceProvider)
            {
                var sqsClient = options.SqsClient
                    ?? (options.Region is not null
                        ? new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region))
                        : serviceProvider.GetService<IAmazonSQS>() ?? new AmazonSQSClient());

                return new SQSSender(sqsClient, name, options.QueueUrl!, options.MessageGroupId!);
            }
        }

        /// <summary>
        /// Adds an <see cref="SQSSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSSenderOptions"/>.</param>
        /// <param name="reloadOnChange">
        /// Whether to create an SQS sender that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddSQSSender(this IServiceCollection services, string name,
            Action<SQSSenderOptions> configureOptions, bool reloadOnChange) =>
            services.AddSQSSender(name, configureOptions, reloadOnChange, _defaultLifetime);

        /// <summary>
        /// Adds an <see cref="SQSSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSSenderOptions"/>.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddSQSSender(this IServiceCollection services, string name,
            Action<SQSSenderOptions> configureOptions) =>
            services.AddSQSSender(name, configureOptions, true, _defaultLifetime);

        /// <summary>
        /// Adds an <see cref="SQSReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSReceiverOptions"/>.</param>
        /// <param name="reloadOnChange">
        /// Whether to create an SQS receiver that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the receiver.</param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddSQSReceiver(this IServiceCollection services, string name,
            Action<SQSReceiverOptions>? configureOptions = null, bool reloadOnChange = true, ServiceLifetime lifetime = _defaultLifetime)
        {
            return services.AddReceiver(name, CreateSQSReceiver, configureOptions, reloadOnChange, lifetime);

            IReceiver CreateSQSReceiver(SQSReceiverOptions options, IServiceProvider serviceProvider)
            {
                var sqsClient = options.SqsClient
                    ?? (options.Region is not null
                        ? new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region))
                        : serviceProvider.GetService<IAmazonSQS>() ?? new AmazonSQSClient());

                if (options.ProcessMessageGroupsConcurrently)
                {
                    return new SQSConcurrentReceiver(sqsClient, name, options.QueueUrl!, options.MaxMessages,
                        options.AutoAcknowledge, options.WaitTimeSeconds, options.UnpackSNS,
                        options.TerminateMessageVisibilityTimeoutOnRollback);
                }

                return new SQSReceiver(sqsClient, name, options.QueueUrl!, options.MaxMessages,
                    options.AutoAcknowledge, options.WaitTimeSeconds, options.UnpackSNS,
                    options.TerminateMessageVisibilityTimeoutOnRollback);
            }
        }

        /// <summary>
        /// Adds an <see cref="SQSReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSReceiverOptions"/>.</param>
        /// <param name="reloadOnChange">
        /// Whether to create an SQS receiver that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddSQSReceiver(this IServiceCollection services, string name,
            Action<SQSReceiverOptions> configureOptions, bool reloadOnChange) =>
            services.AddSQSReceiver(name, configureOptions, reloadOnChange, _defaultLifetime);

        /// <summary>
        /// Adds an <see cref="SQSReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSReceiverOptions"/>.</param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddSQSReceiver(this IServiceCollection services, string name,
            Action<SQSReceiverOptions> configureOptions) =>
            services.AddSQSReceiver(name, configureOptions, true, _defaultLifetime);
    }
}

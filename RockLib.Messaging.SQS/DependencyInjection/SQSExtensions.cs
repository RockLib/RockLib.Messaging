#if !NET451
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
            Action<SQSSenderOptions> configureOptions = null, bool reloadOnChange = true)
        {
            return services.AddSender(name, CreateSQSSender, configureOptions, reloadOnChange);

            ISender CreateSQSSender(SQSSenderOptions options, IServiceProvider serviceProvider)
            {
                var sqsClient = options.SqsClient
                    ?? (options.Region != null
                        ? new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region))
                        : serviceProvider.GetService<IAmazonSQS>() ?? new AmazonSQSClient());

                return new SQSSender(sqsClient, name, options.QueueUrl, options.MessageGroupId);
            }
        }

        /// <summary>
        /// Adds an <see cref="SQSSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSSenderOptions"/>.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddSQSSender(this IServiceCollection services, string name,
            Action<SQSSenderOptions> configureOptions) =>
            services.AddSQSSender(name, configureOptions, true);

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
            Action<SQSReceiverOptions> configureOptions = null, bool reloadOnChange = true)
        {
            return services.AddReceiver(name, CreateSQSReceiver, configureOptions, reloadOnChange);

            IReceiver CreateSQSReceiver(SQSReceiverOptions options, IServiceProvider serviceProvider)
            {
                var sqsClient = options.SqsClient
                    ?? (options.Region != null
                        ? new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region))
                        : serviceProvider.GetService<IAmazonSQS>() ?? new AmazonSQSClient());

                return new SQSReceiver(sqsClient, name, options.QueueUrl, options.MaxMessages,
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
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddSQSReceiver(this IServiceCollection services, string name,
            Action<SQSReceiverOptions> configureOptions) =>
            services.AddSQSReceiver(name, configureOptions, true);
    }
}
#endif

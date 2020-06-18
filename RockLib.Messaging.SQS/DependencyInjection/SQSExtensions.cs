#if !NET451
using Amazon;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        /// Adds a <see cref="SQSSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSSenderOptions"/>.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddSQSSender(this IServiceCollection services, string name, Action<SQSSenderOptions> configureOptions = null)
        {
            return services.AddSender(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<SQSSenderOptions>>();
                var options = optionsMonitor?.Get(name) ?? new SQSSenderOptions();
                configureOptions?.Invoke(options);

                var sqsClient = options.SqsClient
                    ?? (options.Region == null
                        ? new AmazonSQSClient()
                        : new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region)));

                return new SQSSender(sqsClient, name, options.QueueUrl, options.MessageGroupId);
            });
        }

        /// <summary>
        /// Adds a <see cref="SQSReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SQSReceiverOptions"/>.</param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddSQSReceiver(this IServiceCollection services, string name, Action<SQSReceiverOptions> configureOptions = null)
        {
            return services.AddReceiver(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<SQSReceiverOptions>>();
                var options = optionsMonitor?.Get(name) ?? new SQSReceiverOptions();
                configureOptions?.Invoke(options);

                var sqsClient = options.SqsClient
                    ?? (options.Region == null
                        ? new AmazonSQSClient()
                        : new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region)));

                return new SQSReceiver(sqsClient, name, options.QueueUrl, options.MaxMessages,
                    options.AutoAcknowledge, options.WaitTimeSeconds, options.UnpackSNS);
            });
        }
    }
}
#endif

#if !NET451
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

                return new SQSSender(name, options.QueueUrl, options.Region, options.MessageGroupId);
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

                return new SQSReceiver(name, options.QueueUrl, options.Region, options.MaxMessages,
                    options.AutoAcknowledge, options.WaitTimeSeconds, options.UnpackSNS);
            });
        }
    }
}
#endif

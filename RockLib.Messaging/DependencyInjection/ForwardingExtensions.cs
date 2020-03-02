#if !NET451
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering a forwarding receiver.
    /// </summary>
    public static class ForwardingExtensions
    {
        /// <summary>
        /// Adds a <see cref="ForwardingReceiver"/> registration to the builder.
        /// </summary>
        /// <param name="builder">The <see cref="IReceiverBuilder"/>.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="IForwardingReceiverOptions"/>.</param>
        /// <returns>The same <see cref="IReceiverBuilder"/>.</returns>
        public static IReceiverBuilder AddForwardingReceiver(this IReceiverBuilder builder,
            Action<IForwardingReceiverOptions> configureOptions = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddDecorator((receiver, serviceProvider) =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<ForwardingReceiverOptions>>();
                var options = optionsMonitor?.Get(receiver.Name) ?? new ForwardingReceiverOptions();
                configureOptions?.Invoke(options);

                return new ForwardingReceiver(receiver.Name, receiver,
                    options.GetAcknowledgeForwarder(serviceProvider), options.AcknowledgeOutcome,
                    options.GetRollbackForwarder(serviceProvider), options.RollbackOutcome,
                    options.GetRejectForwarder(serviceProvider), options.RejectOutcome);
            });

            return builder;
        }
    }
}
#endif

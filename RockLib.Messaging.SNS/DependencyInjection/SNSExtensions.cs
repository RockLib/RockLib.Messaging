#if !NET451
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RockLib.Messaging.SNS;
using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering SNS senders.
    /// </summary>
    public static class SNSExtensions
    {
        /// <summary>
        /// Adds an <see cref="SNSSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SNSSenderOptions"/>.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddSNSSender(this IServiceCollection services, string name, Action<SNSSenderOptions> configureOptions = null)
        {
            return services.AddSender(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<SNSSenderOptions>>();
                var options = optionsMonitor?.Get(name) ?? new SNSSenderOptions();
                configureOptions?.Invoke(options);

                return new SNSSender(name, options.TopicArn, options.Region);
            });
        }
    }
}
#endif

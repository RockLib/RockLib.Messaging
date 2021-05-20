#if !NET451
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="reloadOnChange">
        /// Whether to create an SNS sender that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddSNSSender(this IServiceCollection services, string name,
            Action<SNSSenderOptions> configureOptions = null, bool reloadOnChange = true)
        {
            return services.AddSender(name, CreateSNSSender, configureOptions, reloadOnChange);

            ISender CreateSNSSender(SNSSenderOptions options, IServiceProvider serviceProvider) =>
                new SNSSender(name, options.TopicArn, options.Region);
        }

        /// <summary>
        /// Adds an <see cref="SNSSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="SNSSenderOptions"/>.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddSNSSender(this IServiceCollection services, string name,
            Action<SNSSenderOptions> configureOptions) =>
            services.AddSNSSender(name, configureOptions, true);
    }
}
#endif

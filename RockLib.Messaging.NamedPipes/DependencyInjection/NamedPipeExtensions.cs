#if !NET451
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RockLib.Messaging.NamedPipes;
using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering named pipe senders and receivers.
    /// </summary>
    public static class NamedPipeExtensions
    {
        /// <summary>
        /// Adds a <see cref="NamedPipeSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="NamedPipeOptions"/>.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddNamedPipeSender(this IServiceCollection services, string name, Action<NamedPipeOptions> configureOptions = null)
        {
            return services.AddSender(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<NamedPipeOptions>>();
                var options = optionsMonitor?.Get(name) ?? new NamedPipeOptions();
                configureOptions?.Invoke(options);

                return new NamedPipeSender(name, options.PipeName);
            });
        }

        /// <summary>
        /// Adds a <see cref="NamedPipeReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="NamedPipeOptions"/>.</param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddNamedPipeReceiver(this IServiceCollection services, string name, Action<NamedPipeOptions> configureOptions = null)
        {
            return services.AddReceiver(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<NamedPipeOptions>>();
                var options = optionsMonitor?.Get(name) ?? new NamedPipeOptions();
                configureOptions?.Invoke(options);

                return new NamedPipeReceiver(name, options.PipeName);
            });
        }
    }
}
#endif

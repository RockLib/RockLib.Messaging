using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="reloadOnChange">
        /// Whether to create a named pipe sender that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddNamedPipeSender(this IServiceCollection services, string name,
            Action<NamedPipeOptions>? configureOptions = null, bool reloadOnChange = true)
        {
            return services.AddSender(name, CreateNamedPipeSender, configureOptions, reloadOnChange);

            ISender CreateNamedPipeSender(NamedPipeOptions options, IServiceProvider serviceProvider) =>
                new NamedPipeSender(name, options.PipeName);
        }

        /// <summary>
        /// Adds a <see cref="NamedPipeSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="NamedPipeOptions"/>.</param>
        /// <returns>A builder allowing the sender to be decorated.</returns>
        public static ISenderBuilder AddNamedPipeSender(this IServiceCollection services, string name,
            Action<NamedPipeOptions> configureOptions) =>
            services.AddNamedPipeSender(name, configureOptions, true);

        /// <summary>
        /// Adds a <see cref="NamedPipeReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="NamedPipeOptions"/>.</param>
        /// <param name="reloadOnChange">
        /// Whether to create a named pipe receiver that automatically reloads itself when its
        /// configuration or options change.
        /// </param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddNamedPipeReceiver(this IServiceCollection services, string name,
            Action<NamedPipeOptions>? configureOptions = null, bool reloadOnChange = true)
        {
            return services.AddReceiver(name, CreateNamedPipeReceiver, configureOptions, reloadOnChange);

            IReceiver CreateNamedPipeReceiver(NamedPipeOptions options, IServiceProvider serviceProvider) =>
                new NamedPipeReceiver(name, options.PipeName);
        }

        /// <summary>
        /// Adds a <see cref="NamedPipeReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="configureOptions">A callback for configuring the <see cref="NamedPipeOptions"/>.</param>
        /// <returns>A builder allowing the receiver to be decorated.</returns>
        public static IReceiverBuilder AddNamedPipeReceiver(this IServiceCollection services, string name,
            Action<NamedPipeOptions> configureOptions) =>
            services.AddNamedPipeReceiver(name, configureOptions, true);
    }
}
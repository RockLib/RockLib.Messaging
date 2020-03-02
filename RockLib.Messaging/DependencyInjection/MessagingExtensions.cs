#if !NET451
using Microsoft.Extensions.DependencyInjection;
using RockLib.Configuration.ObjectFactory;
using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Extension methods for dependency injection and messaging.
    /// </summary>
    public static class MessagingExtensions
    {
        private const ServiceLifetime _defaultLifetime = ServiceLifetime.Singleton;

        /// <summary>
        /// Adds an <see cref="ISender"/> to the service collection where the sender is
        /// created by <see cref="MessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="senderName">
        /// The name that identifies which sender from <see cref="MessagingScenarioFactory.Configuration"/>
        /// to create.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the sender.</param>
        /// <returns>A new <see cref="ISenderBuilder"/> for decorating the <see cref="ISender"/>.</returns>
        public static ISenderBuilder AddSender(this IServiceCollection services, string senderName,
            ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (senderName is null)
                throw new ArgumentNullException(nameof(senderName));

            return services.AddSender(serviceProvider =>
            {
                var resolver = new Resolver(serviceProvider.GetService);
                return MessagingScenarioFactory.CreateSender(senderName, resolver: resolver);
            }, lifetime);
        }

        /// <summary>
        /// Adds an <see cref="ISender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registration">
        /// The registration delegate that is responsible for creating the <see cref="ISender"/>.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the sender.</param>
        /// <returns>A new <see cref="ISenderBuilder"/> for decorating the <see cref="ISender"/>.</returns>
        public static ISenderBuilder AddSender(this IServiceCollection services, SenderRegistration registration,
            ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (registration is null)
                throw new ArgumentNullException(nameof(registration));

            var builder = new SenderBuilder(registration);
            services.Add(new ServiceDescriptor(typeof(ISender), builder.Build, lifetime));
            return builder;
        }

        /// <summary>
        /// Adds an <see cref="ITransactionalSender"/> to the service collection where the sender is
        /// created by <see cref="MessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="senderName">
        /// The name that identifies which sender from <see cref="MessagingScenarioFactory.Configuration"/>
        /// to create.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the transactional sender.</param>
        /// <returns>A new <see cref="ITransactionalSender"/> for decorating the <see cref="ITransactionalSender"/>.</returns>
        public static ITransactionalSenderBuilder AddTransactionalSender(this IServiceCollection services, string senderName,
            ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (senderName is null)
                throw new ArgumentNullException(nameof(senderName));

            return services.AddTransactionalSender(serviceProvider =>
            {
                var resolver = new Resolver(serviceProvider.GetService);
                return (ITransactionalSender)MessagingScenarioFactory.CreateSender(senderName, resolver: resolver);
            }, lifetime);
        }

        /// <summary>
        /// Adds an <see cref="ITransactionalSender"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registration">
        /// The registration delegate that is responsible for creating the <see cref="ITransactionalSender"/>.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the transactional sender.</param>
        /// <returns>A new <see cref="ITransactionalSenderBuilder"/> for decorating the <see cref="ITransactionalSender"/>.</returns>
        public static ITransactionalSenderBuilder AddTransactionalSender(this IServiceCollection services,
            TransactionalSenderRegistration registration, ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (registration is null)
                throw new ArgumentNullException(nameof(registration));

            var builder = new TransactionalSenderBuilder(registration);
            services.Add(new ServiceDescriptor(typeof(ITransactionalSender), builder.Build, lifetime));
            return builder;
        }

        /// <summary>
        /// Adds an <see cref="IReceiver"/> to the service collection where the receiver is
        /// created by <see cref="MessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="receiverName">
        /// The name that identifies which receiver from <see cref="MessagingScenarioFactory.Configuration"/>
        /// to create.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the receiver.</param>
        /// <returns>A new <see cref="IReceiverBuilder"/> for decorating the <see cref="IReceiver"/>.</returns>
        public static IReceiverBuilder AddReceiver(this IServiceCollection services, string receiverName,
            ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (receiverName is null)
                throw new ArgumentNullException(nameof(receiverName));

            return services.AddReceiver(serviceProvider =>
            {
                var resolver = new Resolver(serviceProvider.GetService);
                return MessagingScenarioFactory.CreateReceiver(receiverName, resolver: resolver);
            }, lifetime);
        }

        /// <summary>
        /// Adds an <see cref="IReceiver"/> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registration">
        /// The registration delegate that is responsible for creating the <see cref="IReceiver"/>.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the receiver.</param>
        /// <returns>A new <see cref="IReceiverBuilder"/> for decorating the <see cref="IReceiver"/>.</returns>
        public static IReceiverBuilder AddReceiver(this IServiceCollection services, ReceiverRegistration registration,
            ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (registration is null)
                throw new ArgumentNullException(nameof(registration));

            var builder = new ReceiverBuilder(registration);
            services.Add(new ServiceDescriptor(typeof(IReceiver), builder.Build, lifetime));
            return builder;
        }
    }
}
#endif

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RockLib.Configuration;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Extension methods for dependency injection and messaging.
    /// </summary>
    public static class MessagingExtensions
    {
        /// <summary>The default lifetime of messaging services.</summary>
        private const ServiceLifetime _defaultLifetime = ServiceLifetime.Singleton;

        /// <summary>
        /// Adds an <see cref="ISender"/> to the service collection where the sender is
        /// created from the 'RockLib_Messaging' / 'RockLib.Messaging' composite section
        /// of the registered <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="senderName">
        /// The name that identifies which sender from configuration to create.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the sender.</param>
        /// <returns>A new <see cref="ISenderBuilder"/> for decorating the <see cref="ISender"/>.</returns>
        public static ISenderBuilder AddSender(this IServiceCollection services, string senderName,
            ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (senderName is null)
            {
                throw new ArgumentNullException(nameof(senderName));
            }

            return services.AddSender(serviceProvider =>
            {
                var messagingSection = GetMessagingSection(serviceProvider);
                var resolver = new Resolver(serviceProvider.GetService!, CanResolve);

                return messagingSection.CreateSender(senderName, resolver: resolver);
            }, lifetime);

            bool CanResolve(Type type) =>
                type != typeof(ISender)
                && type != typeof(SenderLookup)
                && services.Any(s => s.ServiceType == type);
        }

        /// <summary>
        /// Adds an <see cref="ISender"/> to the service collection where the sender is created
        /// with options obtained by name from an <see cref="IOptionsMonitor{TOptions}"/> using
        /// the specified factory method.
        /// <para>If <paramref name="reloadOnChange"/> is true, then the sender will automatically
        /// reload itself whenever its options
        /// changes.</para>
        /// </summary>
        /// <typeparam name="TSenderOptions">The type of options used to create the sender.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="senderName">
        /// The name that identifies which named options to use from the <see cref=
        /// "IOptionsMonitor{TOptions}"/>.
        /// </param>
        /// <param name="createSender">
        /// A function that creates the instance of <see cref="ISender"/> using options obtained
        /// from the <see cref="IOptionsMonitor{TOptions}"/>.
        /// </param>
        /// <param name="configureOptions">
        /// An optional callback for configuring the <typeparamref name="TSenderOptions"/>. If
        /// provided, invoking this callback is the <em>last</em> step in creating and configuring
        /// the options used to create the sender.
        /// </param>
        /// <param name="reloadOnChange">
        /// Whether to create a sender that automatically reloads itself when its configuration
        /// or options change.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the sender.</param>
        /// <returns>A new <see cref="ISenderBuilder"/> for decorating the <see cref="ISender"/>.</returns>
        public static ISenderBuilder AddSender<TSenderOptions>(this IServiceCollection services, string senderName,
            Func<TSenderOptions, IServiceProvider, ISender> createSender, Action<TSenderOptions>? configureOptions = null,
            bool reloadOnChange = true, ServiceLifetime lifetime = _defaultLifetime)
            where TSenderOptions : class, new()
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (senderName is null)
            {
                throw new ArgumentNullException(nameof(senderName));
            }
            if (createSender is null)
            {
                throw new ArgumentNullException(nameof(createSender));
            }

            return services.AddSender(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<TSenderOptions>>();

                if (reloadOnChange && optionsMonitor is not null)
                {
                    return new ReloadingSender<TSenderOptions>(senderName,
                        options => createSender.Invoke(options, serviceProvider),
                        optionsMonitor, configureOptions);
                }

                var senderOptions = optionsMonitor!.GetOptions(senderName, configureOptions);
                return createSender.Invoke(senderOptions, serviceProvider);
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
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (registration is null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var builder = new SenderBuilder(registration);

            services.Add(new ServiceDescriptor(typeof(ISender), builder.Build, lifetime));
            services.SetSenderLookupDescriptor();

            return builder;
        }

        /// <summary>
        /// Adds an <see cref="ITransactionalSender"/> to the service collection where the sender is
        /// created from the 'RockLib_Messaging' / 'RockLib.Messaging' composite section
        /// of the registered <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="senderName">
        /// The name that identifies which sender from configuration to create.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the transactional sender.</param>
        /// <returns>A new <see cref="ITransactionalSender"/> for decorating the <see cref="ITransactionalSender"/>.</returns>
        public static ITransactionalSenderBuilder AddTransactionalSender(this IServiceCollection services, string senderName,
            ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (senderName is null)
            {
                throw new ArgumentNullException(nameof(senderName));
            }

            return services.AddTransactionalSender(serviceProvider =>
            {
                var messagingSection = GetMessagingSection(serviceProvider);
                var resolver = new Resolver(serviceProvider.GetService!, CanResolve);

                return (ITransactionalSender)messagingSection.CreateSender(senderName, resolver: resolver);
            }, lifetime);

            bool CanResolve(Type type) =>
                type != typeof(ITransactionalSender)
                && type != typeof(TransactionalSenderLookup)
                && services.Any(s => s.ServiceType == type);
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
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (registration is null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var builder = new TransactionalSenderBuilder(registration);

            services.Add(new ServiceDescriptor(typeof(ITransactionalSender), builder.Build, lifetime));
            services.SetTransactionalSenderLookupDescriptor();
            services.SetSenderLookupDescriptor();

            return builder;
        }

        /// <summary>
        /// Adds an <see cref="IReceiver"/> to the service collection where the receiver is
        /// created from the 'RockLib_Messaging' / 'RockLib.Messaging' composite section
        /// of the registered <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="receiverName">
        /// The name that identifies which receiver from configuration to create.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the receiver.</param>
        /// <returns>A new <see cref="IReceiverBuilder"/> for decorating the <see cref="IReceiver"/>.</returns>
        public static IReceiverBuilder AddReceiver(this IServiceCollection services, string receiverName,
            ServiceLifetime lifetime = _defaultLifetime)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (receiverName is null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            return services.AddReceiver(serviceProvider =>
            {
                var messagingSection = GetMessagingSection(serviceProvider);
                var resolver = new Resolver(serviceProvider.GetService!, CanResolve);

                return messagingSection.CreateReceiver(receiverName, resolver: resolver);
            }, lifetime);

            bool CanResolve(Type type) =>
                type != typeof(IReceiver)
                && type != typeof(ReceiverLookup)
                && services.Any(s => s.ServiceType == type);
        }

        /// <summary>
        /// Adds an <see cref="IReceiver"/> to the service collection where the receiver is created
        /// with options obtained by name from an <see cref="IOptionsMonitor{TOptions}"/> using
        /// the specified factory method.
        /// <para>If <paramref name="reloadOnChange"/> is true, then the receiver will automatically
        /// reload itself whenever its options changes.</para>
        /// </summary>
        /// <typeparam name="TReceiverOptions">The type of options used to create the receiver.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="receiverName">
        /// The name that identifies which named options to use from the <see cref=
        /// "IOptionsMonitor{TOptions}"/>.
        /// </param>
        /// <param name="createReceiver">
        /// A function that creates the instance of <see cref="IReceiver"/> using options obtained
        /// from the <see cref="IOptionsMonitor{TOptions}"/>.
        /// </param>
        /// <param name="configureOptions">
        /// An optional callback for configuring the <typeparamref name="TReceiverOptions"/>. If
        /// provided, invoking this callback is the <em>last</em> step in creating and configuring
        /// the options used to create the receiver.
        /// </param>
        /// <param name="reloadOnChange">
        /// Whether to create a receiver that automatically reloads itself when its configuration
        /// or options change.
        /// </param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the receiver.</param>
        /// <returns>A new <see cref="IReceiverBuilder"/> for decorating the <see cref="IReceiver"/>.</returns>
        public static IReceiverBuilder AddReceiver<TReceiverOptions>(this IServiceCollection services, string receiverName,
            Func<TReceiverOptions, IServiceProvider, IReceiver> createReceiver, Action<TReceiverOptions>? configureOptions = null,
            bool reloadOnChange = true, ServiceLifetime lifetime = _defaultLifetime)
            where TReceiverOptions : class, new()
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (receiverName is null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }
            if (createReceiver is null)
            {
                throw new ArgumentNullException(nameof(createReceiver));
            }

            return services.AddReceiver(serviceProvider =>
            {
                var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<TReceiverOptions>>();

                if (reloadOnChange && optionsMonitor is not null)
                {
                    return new ReloadingReceiver<TReceiverOptions>(receiverName,
                        options => createReceiver.Invoke(options, serviceProvider),
                        optionsMonitor, configureOptions);
                }

                var receiverOptions = optionsMonitor!.GetOptions(receiverName, configureOptions);
                return createReceiver.Invoke(receiverOptions, serviceProvider);
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
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (registration is null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var builder = new ReceiverBuilder(registration);

            services.Add(new ServiceDescriptor(typeof(IReceiver), builder.Build, lifetime));
            services.SetReceiverLookupDescriptor();

            return builder;
        }

        internal static TOptions GetOptions<TOptions>(this IOptionsMonitor<TOptions> optionsMonitor,
            string name, Action<TOptions>? configureOptions)
            where TOptions : class, new()
        {
            var options = optionsMonitor?.Get(name) ?? new TOptions();
            configureOptions?.Invoke(options);
            return options;
        }

        private static IConfigurationSection GetMessagingSection(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            return configuration.GetCompositeSection("RockLib_Messaging", "RockLib.Messaging");
        }

        private static bool NamesEqual(string messagingServiceName, string lookupName)
        {
            if (string.Equals(messagingServiceName, lookupName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (lookupName is null)
            {
                return string.Equals(messagingServiceName, "default", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static void SetSenderLookupDescriptor(this IServiceCollection services)
        {
            // Clear the existing SenderLookup descriptor, if it exists.
            for (var i = 0; i < services.Count; i++)
            {
                if (services[i].ServiceType == typeof(SenderLookup))
                {
                    services.RemoveAt(i--);
                }
            }

            // Capture which senders and which transactional senders are singleton according to index.
            IReadOnlyList<bool> isSingletonSender = services.Where(service => service.ServiceType == typeof(ISender))
                .Select(service => service.Lifetime == ServiceLifetime.Singleton)
                .ToArray();
            IReadOnlyList<bool> isSingletonTransactionalSender = services.Where(service => service.ServiceType == typeof(ITransactionalSender))
                .Select(service => service.Lifetime == ServiceLifetime.Singleton)
                .ToArray();

            SenderLookup SenderFactory(IServiceProvider serviceProvider) => name =>
            {
                // Find the first sender that has a matching name.
                var senders = serviceProvider.GetServices<ISender>().ToArray();
                var selectedSender = senders.FirstOrDefault(sender => NamesEqual(sender.Name, name));

                if (selectedSender is not null)
                {
                    // Immediately dispose any non-singleton senders that weren't selected.
                    for (var i = 0; i < senders.Length; i++)
                    {
                        if (!isSingletonSender[i] && !ReferenceEquals(senders[i], selectedSender))
                        {
                            senders[i].Dispose();
                        }
                    }
                }
                else
                {
                    // Immediately dispose of all non-singleton senders, since none were selected.
                    for (var i = 0; i < senders.Length; i++)
                    {
                        if (!isSingletonSender[i])
                        {
                            senders[i].Dispose();
                        }
                    }

                    // Find the first transactional sender that has a matching name.
                    var transactionalSenders = serviceProvider.GetServices<ITransactionalSender>().ToArray();
                    selectedSender = transactionalSenders.FirstOrDefault(sender => NamesEqual(sender.Name, name));

                    // Immediately dispose any non-singleton transactional senders that weren't selected.
                    for (var i = 0; i < transactionalSenders.Length; i++)
                    {
                        if (!isSingletonTransactionalSender[i] && !ReferenceEquals(transactionalSenders[i], selectedSender))
                        {
                            transactionalSenders[i].Dispose();
                        }
                    }
                }

                return selectedSender!;
            };

            services.AddSingleton(SenderFactory);
        }

        private static void SetTransactionalSenderLookupDescriptor(this IServiceCollection services)
        {
            // Clear the existing TransactionalSenderLookup descriptor, if it exists.
            for (var i = 0; i < services.Count; i++)
            {
                if (services[i].ServiceType == typeof(TransactionalSenderLookup))
                {
                    services.RemoveAt(i--);
                }
            }

            // Capture which transactional senders are singleton according to index.
            IReadOnlyList<bool> isSingletonTransactionalSender = services.Where(service => service.ServiceType == typeof(ITransactionalSender))
                .Select(service => service.Lifetime == ServiceLifetime.Singleton)
                .ToArray();

            TransactionalSenderLookup TransactionalSenderFactory(IServiceProvider serviceProvider) => name =>
            {
                // Find the first transactional sender that has a matching name.
                var transactionalSenders = serviceProvider.GetServices<ITransactionalSender>().ToArray();
                var selectedTransactionalSender = transactionalSenders.FirstOrDefault(sender => NamesEqual(sender.Name, name));

                // Immediately dispose any non-singleton transactional senders that weren't selected.
                for (var i = 0; i < transactionalSenders.Length; i++)
                {
                    if (!isSingletonTransactionalSender[i] && !ReferenceEquals(transactionalSenders[i], selectedTransactionalSender))
                    {
                        transactionalSenders[i].Dispose();
                    }
                }

                return selectedTransactionalSender!;
            };

            services.AddSingleton(TransactionalSenderFactory);
        }

        private static void SetReceiverLookupDescriptor(this IServiceCollection services)
        {
            // Clear the existing ReceiverLookup descriptor, if it exists.
            for (var i = 0; i < services.Count; i++)
            {
                if (services[i].ServiceType == typeof(ReceiverLookup))
                {
                    services.RemoveAt(i--);
                }
            }

            // Capture which receivers are singleton according to index.
            IReadOnlyList<bool> isSingletonReceiver = services.Where(service => service.ServiceType == typeof(IReceiver))
                .Select(service => service.Lifetime == ServiceLifetime.Singleton)
                .ToArray();

            ReceiverLookup ReceiverLookupFactory(IServiceProvider serviceProvider) => name =>
            {
                // Find the first receiver that has a matching name.
                var receivers = serviceProvider.GetServices<IReceiver>().ToArray();
                var selectedReceiver = receivers.FirstOrDefault(receiver => NamesEqual(receiver.Name, name));

                // Immediately dispose any non-singleton receivers that weren't selected.
                for (var i = 0; i < receivers.Length; i++)
                {
                    if (!isSingletonReceiver[i] && !ReferenceEquals(receivers[i], selectedReceiver))
                    {
                        receivers[i].Dispose();
                    }
                }

                return selectedReceiver!;
            };

            services.AddSingleton(ReceiverLookupFactory);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using RockLib.Configuration;
using RockLib.Immutable;
using RockLib.Configuration.ObjectFactory;
using Microsoft.Extensions.Configuration;
using Resolver=RockLib.Configuration.ObjectFactory.Resolver;

namespace RockLib.Messaging
{
    /// <summary>
    /// Provides methods for creating instances of various messaging scenarios from
    /// an <see cref="IConfiguration"/> object.
    /// </summary>
    public static class MessagingScenarioFactory
    {
        private static readonly Semimutable<IConfiguration> _configuration =
            new Semimutable<IConfiguration>(() => Config.Root.GetSection("RockLib.Messaging"));

        /// <summary>
        /// Sets the value of the <see cref="Configuration"/> property. Note that this
        /// method must be called at the beginning of the application. Once the
        /// <see cref="Configuration"/> property has been read from, it cannot be changed.
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetConfiguration(IConfiguration configuration) => _configuration.Value = configuration;

        /// <summary>
        /// Gets the instance of <see cref="IConfiguration"/> used by
        /// <see cref="MessagingScenarioFactory"/> to construct messaging scenarios.
        /// </summary>
        public static IConfiguration Configuration => _configuration.Value;

        /// <summary>
        /// Creates an instance of the <see cref="ISender"/> interface identified by
        /// its name from the 'senders' section of the <see cref="Configuration"/> property.
        /// </summary>
        /// <param name="name">The name that identifies which sender from configuration to create.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <param name="resolver">
        /// An object that can retrieve constructor parameter values that are not found in configuration. This
        /// object is an adapter for dependency injection containers, such as Ninject, Unity, Autofac, or
        /// StructureMap. Consider using the <see cref="Resolver"/> class for this parameter, as it supports
        /// most depenedency injection containers.
        /// </param>
        /// <param name="reloadOnConfigChange">
        /// Whether to create an instance of <see cref="ISender"/> that automatically reloads itself when its
        /// configuration changes. Default is true.
        /// </param>
        /// <returns>A new instance of the <see cref="ISender"/> interface.</returns>
        public static ISender CreateSender(string name,
            DefaultTypes defaultTypes = null, ValueConverters valueConverters = null,
            IResolver resolver = null, bool reloadOnConfigChange = true) =>
            Configuration.CreateSender(name, defaultTypes, valueConverters, resolver, reloadOnConfigChange);

        /// <summary>
        /// Creates an instance of the <see cref="ISender"/> interface identified by
        /// its name from the 'senders' section of the <paramref name="configuration"/> parameter.
        /// </summary>
        /// <param name="configuration">
        /// A configuration object that contains the specified sender in its 'senders' section.
        /// </param>
        /// <param name="name">The name that identifies which sender from configuration to create.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <param name="resolver">
        /// An object that can retrieve constructor parameter values that are not found in configuration. This
        /// object is an adapter for dependency injection containers, such as Ninject, Unity, Autofac, or
        /// StructureMap. Consider using the <see cref="Resolver"/> class for this parameter, as it supports
        /// most depenedency injection containers.
        /// </param>
        /// <param name="reloadOnConfigChange">
        /// Whether to create an instance of <see cref="ISender"/> that automatically reloads itself when its
        /// configuration changes. Default is true.
        /// </param>
        /// <returns>A new instance of the <see cref="ISender"/> interface.</returns>
        public static ISender CreateSender(this IConfiguration configuration, string name,
            DefaultTypes defaultTypes = null, ValueConverters valueConverters = null,
            IResolver resolver = null, bool reloadOnConfigChange = true)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return configuration.CreateScenario<ISender>("senders", name, defaultTypes, valueConverters, resolver, reloadOnConfigChange);
        }

        /// <summary>
        /// Creates an instance of the <see cref="IReceiver"/> interface identified by
        /// its name from the 'receivers' section of the <see cref="Configuration"/> property.
        /// </summary>
        /// <param name="name">The name that identifies which receiver from configuration to create.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <param name="resolver">
        /// An object that can retrieve constructor parameter values that are not found in configuration. This
        /// object is an adapter for dependency injection containers, such as Ninject, Unity, Autofac, or
        /// StructureMap. Consider using the <see cref="Resolver"/> class for this parameter, as it supports
        /// most depenedency injection containers.
        /// </param>
        /// <param name="reloadOnConfigChange">
        /// Whether to create an instance of <see cref="IReceiver"/> that automatically reloads itself when its
        /// configuration changes. Default is true.
        /// </param>
        /// <returns>A new instance of the <see cref="IReceiver"/> interface.</returns>
        public static IReceiver CreateReceiver(string name,
            DefaultTypes defaultTypes = null, ValueConverters valueConverters = null,
            IResolver resolver = null, bool reloadOnConfigChange = true) =>
            Configuration.CreateReceiver(name, defaultTypes, valueConverters, resolver, reloadOnConfigChange);

        /// <summary>
        /// Creates an instance of the <see cref="IReceiver"/> interface identified by
        /// its name from the 'receivers' section of the <paramref name="configuration"/> parameter.
        /// </summary>
        /// <param name="configuration">
        /// A configuration object that contains the specified receiver in its 'receivers' section.
        /// </param>
        /// <param name="name">The name that identifies which receiver from configuration to create.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <param name="resolver">
        /// An object that can retrieve constructor parameter values that are not found in configuration. This
        /// object is an adapter for dependency injection containers, such as Ninject, Unity, Autofac, or
        /// StructureMap. Consider using the <see cref="Resolver"/> class for this parameter, as it supports
        /// most depenedency injection containers.
        /// </param>
        /// <param name="reloadOnConfigChange">
        /// Whether to create an instance of <see cref="IReceiver"/> that automatically reloads itself when its
        /// configuration changes. Default is true.
        /// </param>
        /// <returns>A new instance of the <see cref="IReceiver"/> interface.</returns>
        public static IReceiver CreateReceiver(this IConfiguration configuration, string name,
            DefaultTypes defaultTypes = null, ValueConverters valueConverters = null,
            IResolver resolver = null, bool reloadOnConfigChange = true)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return configuration.CreateScenario<IReceiver>("receivers", name, defaultTypes, valueConverters, resolver, reloadOnConfigChange);
        }

        private static T CreateScenario<T>(this IConfiguration configuration, string sectionName, string scenarioName, DefaultTypes defaultTypes, ValueConverters valueConverters, IResolver resolver, bool reloadOnConfigChange)
        {
            var section = configuration.GetSection(sectionName);

            if (section.IsEmpty())
                throw new KeyNotFoundException($"The '{sectionName}' section is empty.");

            if (section.IsList())
            {
                foreach (var child in section.GetChildren())
                    if (scenarioName.Equals(child.GetSectionName(), StringComparison.OrdinalIgnoreCase))
                        return reloadOnConfigChange
                            ? child.CreateReloadingProxy<T>(defaultTypes, valueConverters, resolver)
                            : child.Create<T>(defaultTypes, valueConverters, resolver);
            }
            else if (scenarioName.Equals(section.GetSectionName(), StringComparison.OrdinalIgnoreCase))
                return reloadOnConfigChange
                    ? section.CreateReloadingProxy<T>(defaultTypes, valueConverters, resolver)
                    : section.Create<T>(defaultTypes, valueConverters, resolver);

            throw new KeyNotFoundException($"No {sectionName} were found matching the name '{scenarioName}'.");
        }

        private static bool IsEmpty(this IConfigurationSection section) =>
            section.Value == null && !section.GetChildren().Any();

        private static bool IsList(this IConfigurationSection section)
        {
            int i = 0;
            foreach (var child in section.GetChildren())
                if (child.Key != i++.ToString())
                    return false;
            return true;
        }

        private static string GetSectionName(this IConfigurationSection section)
        {
            var valueSection = section;

            if (section["type"] != null && !section.GetSection("value").IsEmpty())
                valueSection = section.GetSection("value");

            return valueSection["name"];
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using RockLib.Configuration;
using RockLib.Immutable;
using RockLib.Configuration.ObjectFactory;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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
        /// <returns>A new instance of the <see cref="ISender"/> interface.</returns>
        public static ISender CreateSender(string name) => Configuration.CreateSender(name);

        /// <summary>
        /// Creates an instance of the <see cref="ISender"/> interface identified by
        /// its name from the 'senders' section of the <paramref name="configuration"/> parameter.
        /// </summary>
        /// <param name="configuration">
        /// A configuration object that contains the specified sender in its 'senders' section.
        /// </param>
        /// <param name="name">The name that identifies which sender from configuration to create.</param>
        /// <returns>A new instance of the <see cref="ISender"/> interface.</returns>
        public static ISender CreateSender(this IConfiguration configuration, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return configuration.CreateScenario<ISender>("senders", name);
        }

        /// <summary>
        /// Creates an instance of the <see cref="IReceiver"/> interface identified by
        /// its name from the 'receivers' section of the <see cref="Configuration"/> property.
        /// </summary>
        /// <param name="name">The name that identifies which receiver from configuration to create.</param>
        /// <returns>A new instance of the <see cref="IReceiver"/> interface.</returns>
        public static IReceiver CreateReceiver(string name) => Configuration.CreateReceiver(name);

        /// <summary>
        /// Creates an instance of the <see cref="IReceiver"/> interface identified by
        /// its name from the 'receivers' section of the <paramref name="configuration"/> parameter.
        /// </summary>
        /// <param name="configuration">
        /// A configuration object that contains the specified receiver in its 'receivers' section.
        /// </param>
        /// <param name="name">The name that identifies which receiver from configuration to create.</param>
        /// <returns>A new instance of the <see cref="IReceiver"/> interface.</returns>
        public static IReceiver CreateReceiver(this IConfiguration configuration, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return configuration.CreateScenario<IReceiver>("receivers", name);
        }

        private static T CreateScenario<T>(this IConfiguration configuration, string sectionName, string scenarioName)
        {
            var section = configuration.GetSection(sectionName);

            if (section.IsEmpty())
                throw new KeyNotFoundException($"The '{sectionName}' section is empty.");

            var defaultTypes = configuration.GetDefaultTypes();

            if (section.IsList())
            {
                foreach (var child in section.GetChildren())
                    if (scenarioName.Equals(child.GetSectionName(), StringComparison.OrdinalIgnoreCase))
                        return child.CreateReloadingProxy<T>(defaultTypes);
            }
            else if (scenarioName.Equals(section.GetSectionName(), StringComparison.OrdinalIgnoreCase))
                return section.CreateReloadingProxy<T>(defaultTypes);

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

        private static DefaultTypes GetDefaultTypes(this IConfiguration configuration)
        {
            var defaultTypes = new DefaultTypes();

            if (configuration["defaultSenderType"] != null)
            {
                var defaultSenderType = Type.GetType(configuration["defaultSenderType"]);
                if (defaultSenderType != null && typeof(ISender).GetTypeInfo().IsAssignableFrom(defaultSenderType))
                    defaultTypes.Add(typeof(ISender), defaultSenderType);
            }

            if (configuration["defaultReceiverType"] != null)
            {
                var defaultReceiverType = Type.GetType(configuration["defaultReceiverType"]);
                if (defaultReceiverType != null && typeof(IReceiver).GetTypeInfo().IsAssignableFrom(defaultReceiverType))
                    defaultTypes.Add(typeof(IReceiver), defaultReceiverType);
            }

            return defaultTypes;
        }
    }
}
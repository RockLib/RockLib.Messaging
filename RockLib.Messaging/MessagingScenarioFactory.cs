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
    /// Provides methods for creating instances of various messaging scenarios.
    /// </summary>
    public static class MessagingScenarioFactory
    {
        private static readonly Semimutable<IConfiguration> _configuration =
            new Semimutable<IConfiguration>(() => Config.Root.GetSection("RockLib.Messaging"));

        public static void SetConfiguration(IConfiguration configuration) => _configuration.Value = configuration;

        public static IConfiguration Configuration => _configuration.Value;

        public static ISender CreateSender(string name) => Configuration.CreateSender(name);

        public static ISender CreateSender(this IConfiguration configuration, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var sendersSection = configuration.GetSection("senders");

            if (sendersSection.IsEmpty())
                throw new KeyNotFoundException();

            if (sendersSection.IsList())
                foreach (var senderSection in sendersSection.GetChildren())
                    if (name.Equals(GetSectionName(senderSection), StringComparison.OrdinalIgnoreCase))
                        return senderSection.Create<ISender>(configuration.GetDefaultTypes());
                    else if (name.Equals(GetSectionName(sendersSection), StringComparison.OrdinalIgnoreCase))
                        return sendersSection.Create<ISender>(configuration.GetDefaultTypes());

            throw new KeyNotFoundException();
        }

        public static IReceiver CreateReceiver(string name) => Configuration.CreateReceiver(name);

        public static IReceiver CreateReceiver(this IConfiguration configuration, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var receiversSection = configuration.GetSection("receivers");

            if (receiversSection.IsEmpty())
                throw new KeyNotFoundException();

            if (receiversSection.IsList())
                foreach (var receiverSection in receiversSection.GetChildren())
                    if (name.Equals(GetSectionName(receiverSection), StringComparison.OrdinalIgnoreCase))
                        return receiverSection.Create<IReceiver>(configuration.GetDefaultTypes());
                    else if (name.Equals(GetSectionName(receiversSection), StringComparison.OrdinalIgnoreCase))
                        return receiversSection.Create<IReceiver>(configuration.GetDefaultTypes());

            throw new KeyNotFoundException();
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
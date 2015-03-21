using System;
using Rock.Defaults;
using System.Configuration;
using Rock.Messaging.Configuration;
using Rock.Messaging.NamedPipes;

namespace Rock.Messaging.Defaults.Implementation
{
    public static partial class Default
    {
        private static readonly Lazy<NamedPipeMessagingScenarioFactory> _fallbackMessagingScenarioFactory = new Lazy<NamedPipeMessagingScenarioFactory>(() => new NamedPipeMessagingScenarioFactory());
        private static readonly DefaultHelper<IMessagingScenarioFactory> _messagingScenarioFactory = new DefaultHelper<IMessagingScenarioFactory>(CreateDefaultMessagingScenarioFactory);

        public static IMessagingScenarioFactory MessagingScenarioFactory
        {
            get { return _messagingScenarioFactory.Current; }
        }

        public static IMessagingScenarioFactory DefaultMessagingScenarioFactory
        {
            get { return _messagingScenarioFactory.DefaultInstance; }
        }

        public static void SetMessagingScenarioFactory(Func<IMessagingScenarioFactory> getMessagingScenarioFactoryInstance)
        {
            _messagingScenarioFactory.SetCurrent(getMessagingScenarioFactoryInstance);
        }

        public static void RestoreDefaultMessagingScenarioFactory()
        {
            _messagingScenarioFactory.RestoreDefault();
        }

        internal static bool WasMessagingScenarioFactoryLoadedFromConfig
        {
            get
            {
                return !_fallbackMessagingScenarioFactory.IsValueCreated;
            }
        }

        private static IMessagingScenarioFactory CreateDefaultMessagingScenarioFactory()
        {
            IMessagingScenarioFactory value;

            return
                TryGetFactoryFromConfig(out value)
                    ? value
                    : _fallbackMessagingScenarioFactory.Value;
        }

        private static bool TryGetFactoryFromConfig(out IMessagingScenarioFactory factory)
        {
            try
            {
                var rockMessagingConfiguration = (IRockMessagingConfiguration)ConfigurationManager.GetSection("rock.messaging");
                factory = rockMessagingConfiguration.MessagingScenarioFactory;
                return true;
            }
            catch (Exception)
            {
                factory = null;
                return false;
            }
        }
    }
}

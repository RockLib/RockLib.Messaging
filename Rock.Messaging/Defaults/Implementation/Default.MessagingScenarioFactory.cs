using System;
using Rock.Defaults;
using System.Configuration;

namespace Rock.Messaging.Defaults.Implementation
{
    public static partial class Default
    {
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

        private static IMessagingScenarioFactory CreateDefaultMessagingScenarioFactory()
        {
            // TODO: Finish implemening the mechanism for getting the default messaging scenario factory.
            return
                (IMessagingScenarioFactory)ConfigurationManager.GetSection("rock.messaging");
                //?? new SimpleLoggerFactory<ConsoleLogProvider>(LogLevel.Debug);
        }
    }
}

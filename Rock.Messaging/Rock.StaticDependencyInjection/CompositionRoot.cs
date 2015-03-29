using System.Configuration;
using Rock.Messaging.NamedPipes;
using Rock.Messaging.Routing;

namespace Rock.Messaging.Rock.StaticDependencyInjection
{
    internal partial class CompositionRoot : CompositionRootBase
    {
        public override void Bootstrap()
        {
            ImportFirst<IMessageParser>(DefaultMessageParser.SetCurrent);
            ImportFirst<INamedPipeConfigProvider>(NamedPipeMessagingScenarioFactory.SetDefaultConfigProvider);
            ImportFirst<ITypeLocator>(DefaultTypeLocator.SetCurrent);
            ImportFirst<IMessagingScenarioFactory>(MessagingScenarioFactory.SetFallback);
        }

        /// <summary>
        /// Gets a value indicating whether static dependency injection is enabled.
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                const string key = "Rock.StaticDependencyInjection.Enabled";
                var enabledValue = ConfigurationManager.AppSettings.Get(key) ?? "true";
                return enabledValue.ToLower() != "false";
            }
        }
    }
}

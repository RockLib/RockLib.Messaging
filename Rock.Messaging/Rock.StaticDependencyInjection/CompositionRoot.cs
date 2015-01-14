using System.Configuration;
using Rock.Messaging.Routing;

namespace Rock.Messaging.Rock.StaticDependencyInjection
{
    internal partial class CompositionRoot : CompositionRootBase
    {
        public override void Bootstrap()
        {
            ImportFirst<IMessageParser>(parser => MessageRouter.DefaultMessageParser = parser);
            ImportFirst<ITypeLocator>(locator => MessageRouter.DefaultTypeLocator = locator);
        }

        /// <summary>
        /// Gets a value indicating whether static dependency injection is enabled.
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                const string key = "Rock.Messaging.StaticDependencyInjection.Enabled";
                var enabledValue = ConfigurationManager.AppSettings.Get(key) ?? "true";
                return enabledValue.ToLower() != "false";
            }
        }
    }
}

using System.Configuration;
using Rock.Messaging.Defaults.Implementation;
using Rock.Messaging.Routing;

namespace Rock.Messaging.Rock.StaticDependencyInjection
{
    internal partial class CompositionRoot : CompositionRootBase
    {
        public override void Bootstrap()
        {
            ImportFirst<IMessageParser>(x => Default.SetMessageParser(() => x));
            ImportFirst<ITypeLocator>(x => Default.SetTypeLocator(() => x));
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

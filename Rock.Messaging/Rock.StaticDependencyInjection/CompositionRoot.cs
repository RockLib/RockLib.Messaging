using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Rock.BackgroundErrorLogging;
using Rock.Messaging.NamedPipes;
using Rock.Messaging.Routing;
using Rock.StaticDependencyInjection;

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
            ImportFirst<IMessageCompressor>(DefaultMessageCompressor.SetCurrent);
        }

        protected override void OnError(string message, Exception exception, ImportInfo import)
        {
            BackgroundErrorLogger.Log(exception, "Static Dependency Injection - " + message, "Rock.Messaging", "ImportInfo:\r\n" + import);

            base.OnError(message, exception, import);
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

        /// <summary>
        /// Return a collection of metadata objects that describe the export operations for a type.
        /// </summary>
        /// <param name="type">The type to get export metadata.</param>
        /// <returns>A collection of metadata objects that describe export operations.</returns>
        protected override IEnumerable<ExportInfo> GetExportInfos(Type type)
        {
            // Modify this method if your library needs to support a different
            // export mechanism (possibly a different attribute) that inspects
            // the type of a class.
            //
            // Remove this method if your library should not support any advanced
            // export mechanisms based on the type of a class.

            var attributes = Attribute.GetCustomAttributes(type, typeof(ExportAttribute));

            if (attributes.Length == 0)
            {
                return base.GetExportInfos(type);
            }

            return
                attributes.Cast<ExportAttribute>()
                .Select(attribute =>
                    new ExportInfo(type, attribute.Priority)
                    {
                        Disabled = attribute.Disabled,
                        Name = attribute.Name
                    });
        }
    }
}

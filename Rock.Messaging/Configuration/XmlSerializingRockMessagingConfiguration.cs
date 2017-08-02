using System;
using System.Linq;
using System.Xml.Serialization;
using Rock.Messaging;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    public class XmlSerializingRockMessagingConfiguration : IRockMessagingConfiguration
    {
        private readonly Lazy<IMessagingScenarioFactory> _messagingScenarioFactory;

        public XmlSerializingRockMessagingConfiguration()
        {
            _messagingScenarioFactory = new Lazy<IMessagingScenarioFactory>(CreateMessagingScenarioFactory);
        }

        [XmlElement("messagingScenarioFactory")]
        public MessagingScenarioFactoryProxy[] FactoryProxies { get; set; }

        [XmlIgnore]
        public IMessagingScenarioFactory MessagingScenarioFactory
        {
            get { return _messagingScenarioFactory.Value; }
        }

        private IMessagingScenarioFactory CreateMessagingScenarioFactory()
        {
            if (FactoryProxies == null)
            {
                throw new InvalidOperationException("FactoryProxies property must not be null.");
            }

            var factories = FactoryProxies.Select(f => f.CreateInstance()).ToArray();

            if (factories.Length == 1)
            {
                return factories[0];
            }

            if (factories.Length > 1)
            {
                return new CompositeMessagingScenarioFactory(factories);
            }

            throw new InvalidOperationException("FactoryProxies must have at least one element.");
        }
    }
}
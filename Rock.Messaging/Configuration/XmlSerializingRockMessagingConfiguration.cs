using System;
using System.Xml.Serialization;
using Rock.Messaging.Configuration;

namespace Rock.Messaging
{
    public class XmlSerializingRockMessagingConfiguration : IRockMessagingConfiguration
    {
        private readonly Lazy<IMessagingScenarioFactory> _messagingScenarioFactory;

        public XmlSerializingRockMessagingConfiguration()
        {
            _messagingScenarioFactory = new Lazy<IMessagingScenarioFactory>(() => FactoryProxy.CreateInstance());
        }

        [XmlElement("messagingScenarioFactory")]
        public MessagingScenarioFactoryProxy FactoryProxy { get; set; }

        [XmlIgnore]
        public IMessagingScenarioFactory MessagingScenarioFactory
        {
            get { return _messagingScenarioFactory.Value; }
        }
    }
}
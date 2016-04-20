using System;
using System.Linq;
using System.Xml.Serialization;

namespace Rock.Messaging.RabbitMQ
{
    public class XmlDeserializingRabbitSessionConfigurationProvider : IRabbitSessionConfigurationProvider
    {
        private RabbitSessionConfiguration[] configurations = new RabbitSessionConfiguration[0];

        [XmlElement("rabbit")]
        public RabbitSessionConfiguration[] Configurations {
            get { return configurations; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                configurations = value;
            } }

        public IRabbitSessionConfiguration GetConfiguration(string name)
        {
            return configurations.FirstOrDefault(c => string.Equals(c.Name, name));
        }

        public bool HasConfiguration(string name)
        {
            return configurations.Any(c => string.Equals(c.Name, name));
        }
    }
}
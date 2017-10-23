using System;
using System.Linq;
using System.Xml.Serialization;

namespace Rock.Messaging.SQS
{
    public class XmlDeserializingSQSConfigurationProvider : ISQSConfigurationProvider
    {
        private XmlSQSConfiguration[] _configurations = new XmlSQSConfiguration[0];

        [XmlElement("sqs")]
        public XmlSQSConfiguration[] Configurations
        {
            get { return _configurations; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _configurations = value;
            }
        }

        public ISQSConfiguration GetConfiguration(string name)
        {
            return _configurations.First(c => c.Name == name);
        }

        public bool HasConfiguration(string name)
        {
            return _configurations.Any(c => c.Name == name);
        }

        public void Validate()
        {
            foreach (var configuration in _configurations)
            {
                configuration.Validate();
            }

            if (_configurations.Select(c => c.Name).Distinct().Count() != _configurations.Length)
            {
                throw new Exception("Each configuration Name must be unique.");
            }
        }
    }
}
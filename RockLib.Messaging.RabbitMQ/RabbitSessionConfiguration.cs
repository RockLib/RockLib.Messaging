using System.Xml.Serialization;

namespace Rock.Messaging.RabbitMQ
{
    public class RabbitSessionConfiguration : IRabbitSessionConfiguration
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("queueName")]
        public string QueueName { get; set; }

        [XmlAttribute("autoAcknowledge")]
        public bool AutoAcknowledge { get; set; }
        [XmlAttribute("routingKey")]
        public string RoutingKey { get; set; }

        [XmlAttribute("exchangeName")]
        public string Exchange { get; set; }
        [XmlAttribute("exchangeType")]
        public string ExchangeType { get; set; }

        [XmlAttribute("maxParallelRequests")]
        public ushort MaxRequests { get; set; }
    }
}
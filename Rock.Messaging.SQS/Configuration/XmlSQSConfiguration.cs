using System;
using System.Xml.Serialization;

namespace Rock.Messaging.SQS
{
    public class XmlSQSConfiguration : ISQSConfiguration
    {
        public XmlSQSConfiguration()
        {
            MaxMessages = 3;
            AutoAcknowledge = true;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("queueUrl")]
        public string QueueUrl { get; set; }

        [XmlAttribute("maxMessages")]
        public int MaxMessages { get; set; }

        [XmlAttribute("autoAcknowledge")]
        public bool AutoAcknowledge { get; set; }

        [XmlAttribute("compressed")]
        public bool Compressed { get; set; }

        [XmlAttribute("parallelHandling")]
        public bool ParallelHandling { get; set; }

        public void Validate()
        {
            if (Name == null)
            {
                throw new Exception("Name must be set.");
            }

            if (QueueUrl == null)
            {
                throw new Exception("QueueUrl must be set.");
            }

            if (MaxMessages < 1)
            {
                throw new Exception("MaxMessages must be greater than zero.");
            }
        }
    }
}

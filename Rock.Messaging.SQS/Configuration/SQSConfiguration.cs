using System;

namespace RockLib.Messaging.SQS
{
    public class SQSConfiguration: ISQSConfiguration
    {
        public SQSConfiguration()
        {
            MaxMessages = 3;
            AutoAcknowledge = true;
        }

        public string Name { get; set; }
        public string QueueUrl { get; set; }
        public int MaxMessages { get; set; }
        public bool AutoAcknowledge { get; set; }
        public bool Compressed { get; set; }

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

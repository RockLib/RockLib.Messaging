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

        public String Name { get; set; }
        public String QueueUrl { get; set; }
        public Int32 MaxMessages { get; set; }
        public Boolean AutoAcknowledge { get; set; }
        public Boolean Compressed { get; set; }
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

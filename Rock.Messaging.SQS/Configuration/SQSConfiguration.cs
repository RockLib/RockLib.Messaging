using System;

namespace RockLib.Messaging.SQS
{
    public class SQSConfiguration : ISQSConfiguration
    {
        private int _maxMessages = 3;

        public SQSConfiguration(string name, string queueUrl)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            QueueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
            AutoAcknowledge = true;
        }

        public string Name { get; }
        public string QueueUrl { get; }
        public int MaxMessages { get => _maxMessages; set => _maxMessages = value > 0 ? value : throw new ArgumentException("MaxMessages must be greater than zero.", nameof(value)); }
        public bool AutoAcknowledge { get; set; }
        public bool Compressed { get; set; }
        public bool ParallelHandling { get; set; }
    }
}

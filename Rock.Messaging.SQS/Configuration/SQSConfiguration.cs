using System;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// A simple implementation of <see cref="ISQSConfiguration"/>.
    /// </summary>
    public class SQSConfiguration : ISQSConfiguration
    {
        private int _maxMessages = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSConfiguration"/> class.
        /// </summary>
        /// <param name="name">The configuration name.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        public SQSConfiguration(string name, string queueUrl)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            QueueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
            AutoAcknowledge = true;
        }

        /// <summary>
        /// Gets the configuration name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the url of the SQS queue.
        /// </summary>
        public string QueueUrl { get; }

        /// <summary>
        /// Gets or sets the maximum number of messages to return with each call to the SQS endpoint.
        /// Amazon SQS never returns more messages than this value (however, fewer messages
        /// might be returned). Valid values are 1 to 10. Default is 3.
        /// </summary>
        public int MaxMessages
        {
            get => _maxMessages;
            set => _maxMessages =
                value >= 1 && value <= 10
                    ? value
                    : throw new ArgumentException("MaxMessages must be a number from one to ten.", nameof(value));
        }


        /// <summary>
        /// Gets or sets a value indicating whether messages will be automatically acknowledged after
        /// anyevent handler execute. Default value is true.
        /// </summary>
        public bool AutoAcknowledge { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether message payloads should be compressed before
        /// sending.
        /// </summary>
        public bool Compressed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether, in the case of when multiple messages are
        /// received from an SQS request, messages are handled in parallel or sequentially.
        /// </summary>
        public bool ParallelHandling { get; set; }
    }
}

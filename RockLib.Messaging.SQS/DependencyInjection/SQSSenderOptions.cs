using Amazon.SQS;
using RockLib.Messaging.SQS;
using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="SQSSender"/>.
    /// </summary>
    public class SQSSenderOptions
    {
        private Uri? _queueUrl;

        /// <summary>
        /// Gets or sets the url of the SQS queue.
        /// </summary>
        public Uri? QueueUrl
        {
            get => _queueUrl;
            set => _queueUrl = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the object that communicates with SQS.
        /// </summary>
        public IAmazonSQS? SqsClient { get; set; }

        /// <summary>
        /// Gets or sets the region of the SQS client.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the tag that specifies that a message belongs to a specific message
        /// group. Messages that belong to the same message group are processed in a FIFO manner
        /// (however, messages in different message groups might be processed out of order). To
        /// interleave multiple ordered streams within a single queue, use MessageGroupId values
        /// (for example, session data for multiple users). In this scenario, multiple consumers
        /// can process the queue, but the session data of each user is processed in a FIFO
        /// fashion.
        /// <para>This parameter applies only to FIFO (first-in-first-out) queues.</para>
        /// </summary>
        public string? MessageGroupId { get; set; }
    }
}

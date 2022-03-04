using Amazon.SQS;
using RockLib.Messaging.SQS;
using System;
using System.Threading;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="SQSReceiver"/>.
    /// </summary>
    public class SQSReceiverOptions
    {
        private Uri? _queueUrl;
        private int _maxMessages = SQSReceiver.DefaultMaxMessages;
        private int _waitTimeSeconds = SQSReceiver.DefaultWaitTimeSeconds;

        /// <summary>
        /// Gets or sets the object that communicates with SQS.
        /// </summary>
        public IAmazonSQS? SqsClient { get; set; }

        /// <summary>
        /// Gets or sets the url of the SQS queue.
        /// </summary>
        public Uri? QueueUrl
        {
            get => _queueUrl;
            set => _queueUrl = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the region of the SQS client.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of messages to return with each call to the SQS
        /// endpoint. Amazon SQS never returns more messages than this value (however, fewer
        /// messages might be returned). Valid values are 1 to 10.
        /// </summary>
        public int MaxMessages
        {
            get => _maxMessages;
            set
            {
                if (value < 1 || value > 10)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be from 1 to 10, inclusive.");
                }
                _maxMessages = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether messages will be automatically acknowledged
        /// after the message handler executes.
        /// </summary>
        public bool AutoAcknowledge { get; set; } = true;

        /// <summary>
        /// Gets or sets the duration (in seconds) for which calls to ReceiveMessage wait for a
        /// message to arrive in the queue before returning. If a message is available, the call
        /// returns sooner than WaitTimeSeconds. If no messages are available and the wait time
        /// expires, the call returns successfully with an empty list of messages.
        /// </summary>
        public int WaitTimeSeconds
        {
            get => _waitTimeSeconds;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative.");
                }
                _waitTimeSeconds = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to attempt to unpack the message body as an
        /// SNS message.
        /// </summary>
        public bool UnpackSNS { get; set; }

        /// <summary>
        /// Gets a value indicating whether to terminate the message visibility timeout when
        /// <see cref="SQSReceiverMessage.RollbackMessageAsync(CancellationToken)"/> is called.
        /// Terminating the message visibility timeout allows the message to immediately become
        /// available for queue consumers to process.
        /// </summary>
        public bool TerminateMessageVisibilityTimeoutOnRollback { get; set; }
    }
}

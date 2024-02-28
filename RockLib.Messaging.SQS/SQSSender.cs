using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that sends messages to SQS.
    /// </summary>
    public class SQSSender : ISender
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
        /// Uses a default implementation of the <see cref="AmazonSQSClient"/> to
        /// communicate with SQS.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        /// <param name="region">The region of the SQS queue.</param>
        public SQSSender(string name, Uri queueUrl, string region)
            : this(name, queueUrl, region, null!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
        /// Uses a default implementation of the <see cref="AmazonSQSClient"/> to
        /// communicate with SQS.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        /// <param name="region">The region of the SQS queue.</param>
        /// <param name="messageGroupId">
        /// The tag that specifies that a message belongs to a specific message group. Messages
        /// that belong to the same message group are processed in a FIFO manner (however,
        /// messages in different message groups might be processed out of order). To interleave
        /// multiple ordered streams within a single queue, use MessageGroupId values (for
        /// example, session data for multiple users). In this scenario, multiple consumers
        /// can process the queue, but the session data of each user is processed in a FIFO
        /// fashion.
        /// <para>This parameter applies only to FIFO (first-in-first-out) queues.</para>
        /// </param>
        public SQSSender(string name, Uri queueUrl, string? region = null, string? messageGroupId = null)
           : this(region is null ? new AmazonSQSClient() : new AmazonSQSClient(RegionEndpoint.GetBySystemName(region)), name, queueUrl, messageGroupId!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
        /// </summary>
        /// <param name="sqs">An object that communicates with SQS.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        public SQSSender(IAmazonSQS sqs, string name, Uri queueUrl)
            : this(sqs, name, queueUrl, null!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
        /// </summary>
        /// <param name="sqs">An object that communicates with SQS.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        /// <param name="messageGroupId">
        /// The tag that specifies that a message belongs to a specific message group. Messages
        /// that belong to the same message group are processed in a FIFO manner (however,
        /// messages in different message groups might be processed out of order). To interleave
        /// multiple ordered streams within a single queue, use MessageGroupId values (for
        /// example, session data for multiple users). In this scenario, multiple consumers
        /// can process the queue, but the session data of each user is processed in a FIFO
        /// fashion.
        /// <para>This parameter applies only to FIFO (first-in-first-out) queues.</para>
        /// </param>
        public SQSSender(IAmazonSQS sqs, string name, Uri queueUrl, string messageGroupId)
        {
            SQSClient = sqs ?? throw new ArgumentNullException(nameof(sqs));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            QueueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
            MessageGroupId = messageGroupId;
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="SQSSender"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the url of the SQS queue.
        /// </summary>
        public Uri QueueUrl { get; }

        /// <summary>
        /// Gets the tag that specifies that a message belongs to a specific message group.
        /// Messages that belong to the same message group are processed in a FIFO manner
        /// (however, messages in different message groups might be processed out of order). To
        /// interleave multiple ordered streams within a single queue, use MessageGroupId values
        /// (for example, session data for multiple users). In this scenario, multiple consumers
        /// can process the queue, but the session data of each user is processed in a FIFO
        /// fashion.
        /// <para>This parameter applies only to FIFO (first-in-first-out) queues.</para>
        /// </summary>
        public string MessageGroupId { get; }

        /// <summary>
        /// Gets the object that communicates with SQS.
        /// </summary>
        public IAmazonSQS SQSClient { get; }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(message);
#else
            if (message is null) { throw new ArgumentNullException(nameof(message)); }
#endif

            if (message.OriginatingSystem is null)
            {
                message.OriginatingSystem = "SQS";
            }

            var sendMessageRequest = new SendMessageRequest(QueueUrl.OriginalString, message.StringPayload);

            if (message.Headers.TryGetValue("SQS.MessageGroupId", out var value) && value is not null)
            {
                sendMessageRequest.MessageGroupId = value.ToString();
                message.Headers.Remove("SQS.MessageGroupId");
            }
            else if (MessageGroupId is not null)
            {
                sendMessageRequest.MessageGroupId = MessageGroupId;
            }

            if (message.Headers.TryGetValue("SQS.MessageDeduplicationId", out value) && value is not null)
            {
                sendMessageRequest.MessageDeduplicationId = value.ToString();
                message.Headers.Remove("SQS.MessageDeduplicationId");
            }

            if (message.Headers.TryGetValue("SQS.DelaySeconds", out value) && value is not null)
            {
                sendMessageRequest.DelaySeconds = (int)value;
                message.Headers.Remove("SQS.DelaySeconds");
            }

            foreach (var header in message.Headers)
            {
                sendMessageRequest.MessageAttributes[header.Key] =
                    new MessageAttributeValue { StringValue = header.Value.ToString(), DataType = "String" };
            }

            return SQSClient.SendMessageAsync(sendMessageRequest, cancellationToken);
        }

        /// <summary>
        /// Disposes managed resources
        /// </summary>
        /// <param name="disposing">Is this being disposed from <see cref="Dispose()"/> or the finalizer?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SQSClient.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the backing <see cref="IAmazonSQS"/> field.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

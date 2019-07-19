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
        private readonly string _queueUrl;
        private readonly IAmazonSQS _sqs;
        private readonly string _messageGroupId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
        /// Uses a default implementation of the <see cref="AmazonSQSClient"/> to
        /// communicate with SQS.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        /// <param name="region">The region of the SQS queue.</param>
        public SQSSender(string name, string queueUrl, string region)
            : this(name, queueUrl, region, null)
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
        public SQSSender(string name, string queueUrl, string region = null, string messageGroupId = null)
           : this(region == null ? new AmazonSQSClient() : new AmazonSQSClient(RegionEndpoint.GetBySystemName(region)), name, queueUrl, messageGroupId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
        /// </summary>
        /// <param name="sqs">An object that communicates with SQS.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        public SQSSender(IAmazonSQS sqs, string name, string queueUrl)
            : this(sqs, name, queueUrl, null)
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
        public SQSSender(IAmazonSQS sqs, string name, string queueUrl, string messageGroupId)
        {
            _sqs = sqs ?? throw new ArgumentNullException(nameof(sqs));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _queueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
            _messageGroupId = messageGroupId;
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="SQSSender"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "SQS";

            var sendMessageRequest = new SendMessageRequest(_queueUrl, message.StringPayload);

            if (message.Headers.TryGetValue("SQS.MessageGroupId", out var value) && value != null)
            {
                sendMessageRequest.MessageGroupId = value.ToString();
                message.Headers.Remove("SQS.MessageGroupId");
            }
            else if (_messageGroupId != null)
                sendMessageRequest.MessageGroupId = _messageGroupId;

            if (message.Headers.TryGetValue("SQS.MessageDeduplicationId", out value) && value != null)
            {
                sendMessageRequest.MessageDeduplicationId = value.ToString();
                message.Headers.Remove("SQS.MessageDeduplicationId");
            }

            foreach (var header in message.Headers)
            {
                sendMessageRequest.MessageAttributes[header.Key] =
                    new MessageAttributeValue { StringValue = header.Value.ToString(), DataType = "String" };
            }

            return _sqs.SendMessageAsync(sendMessageRequest, cancellationToken);
        }

        /// <summary>
        /// Disposes the backing <see cref="IAmazonSQS"/> field.
        /// </summary>
        public void Dispose()
        {
            _sqs.Dispose();
        }
    }
}

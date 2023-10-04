using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace RockLib.Messaging.SNS
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that sends messages to Amazon SNS.
    /// </summary>
    public class SNSSender : ISender
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SNSSender"/> class.
        /// Uses a default implementation of the <see cref="IAmazonSimpleNotificationService"/> to
        /// communicate with SNS.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topicArn">The arn of the SNS topic.</param>
        /// <param name="region">The region of the SNS topic.</param>
        public SNSSender(string name, string topicArn, string? region = null)
            : this(region is null ? new AmazonSimpleNotificationServiceClient() : new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(region)), name, topicArn)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SNSSender"/> class.
        /// </summary>
        /// <param name="client">An object that communicates with SNS.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topicArn">The arn of the SNS topic.</param>
        public SNSSender(IAmazonSimpleNotificationService client, string name, string topicArn)
        {
            SnsClient = client ?? throw new ArgumentNullException(nameof(client));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TopicArn = topicArn ?? throw new ArgumentNullException(nameof(topicArn));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SNSSender"/> class.
        /// </summary>
        /// <param name="client">An object that communicates with SNS.</param>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topicArn">The arn of the SNS topic.</param>
        /// <param name="messageGroupId">
        /// The tag that specifies that a message belongs to a specific message group. Messages
        /// that belong to the same message group are processed in a FIFO manner (however,
        /// messages in different message groups might be processed out of order). To interleave
        /// multiple ordered streams within a single topic, use MessageGroupId values (for
        /// example, session data for multiple users). In this scenario, multiple consumers
        /// can process the topic, but the session data of each user is processed in a FIFO
        /// fashion.
        /// <para>This parameter applies only to FIFO (first-in-first-out) topic.</para>
        /// </param>
        public SNSSender(IAmazonSimpleNotificationService client, string name, string topicArn, string messageGroupId)
        {
            SnsClient = client ?? throw new ArgumentNullException(nameof(client));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TopicArn = topicArn ?? throw new ArgumentNullException(nameof(topicArn));
            MessageGroupId = messageGroupId;
        }

        /// <summary>
        /// Gets the simple notification service client.
        /// </summary>
        public IAmazonSimpleNotificationService SnsClient { get; }

        /// <summary>
        /// Gets the name of this instance of <see cref="SNSSender"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the topic's amazon resource name.
        /// </summary>
        public string TopicArn { get; }

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
        public string? MessageGroupId { get; }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.OriginatingSystem is null)
            {
                message.OriginatingSystem = "SNS";
            }

            var publishMessage = new PublishRequest(TopicArn, message.StringPayload);

            if (message.Headers.TryGetValue("SNS.MessageGroupId", out var value) && value is not null)
            {
                publishMessage.MessageGroupId = value.ToString();
                message.Headers.Remove("SNS.MessageGroupId");
            }
            else if (MessageGroupId is not null)
            {
                publishMessage.MessageGroupId = MessageGroupId;
            }

            foreach (var header in message.Headers)
            {
                publishMessage.MessageAttributes[header.Key] =
                    new MessageAttributeValue { StringValue = header.Value.ToString(), DataType = "String" };
            }

            return SnsClient.PublishAsync(publishMessage, cancellationToken);
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
                    SnsClient.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the backing <see cref="IAmazonSimpleNotificationService"/> field.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

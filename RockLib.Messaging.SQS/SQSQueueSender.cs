using Amazon.SQS;
using Amazon.SQS.Model;
using System.Threading.Tasks;
using System;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that sends messages to SQS.
    /// </summary>
    public class SQSQueueSender : ISender
    {
        private readonly string _name;
        private readonly string _queueUrl;
        private readonly IAmazonSQS _sqs;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSQueueReceiver"/> class.
        /// Uses a default implementation of the <see cref="AmazonSQSClient"/> to
        /// communicate with SQS.
        /// </summary>
        /// <param name="name">The configuration name.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        public SQSQueueSender(string name, string queueUrl)
            : this(new AmazonSQSClient(), name, queueUrl)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSQueueReceiver"/> class.
        /// </summary>
        /// <param name="sqs">An object that communicates with SQS.</param>
        /// <param name="name">The configuration name.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        public SQSQueueSender(IAmazonSQS sqs, string name, string queueUrl)
        {
            _sqs = sqs ?? throw new ArgumentNullException(nameof(sqs));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _queueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="SQSQueueSender"/>.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public Task SendAsync(SenderMessage message)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "SQS";

            var sendMessageRequest = new SendMessageRequest(_queueUrl, message.StringPayload);

            foreach (var header in message.Headers)
            {
                sendMessageRequest.MessageAttributes[header.Key] =
                    new MessageAttributeValue { StringValue = header.Value.ToString(), DataType = "String" };
            }

            return _sqs.SendMessageAsync(sendMessageRequest);
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

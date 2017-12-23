using Amazon.SQS;
using Amazon.SQS.Model;
using System.Threading.Tasks;
using System;

#if ROCKLIB
using RockLib.Messaging.Internal;
#else
using Rock.Messaging.Internal;
#endif

#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that sends messages to SQS.
    /// </summary>
    public class SQSQueueSender : ISender
    {
        private readonly string _name;
        private readonly string _queueUrl;
        private readonly IAmazonSQS _sqs;
        private readonly bool _compressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSQueueReceiver"/> class.
        /// </summary>
        /// <param name="configuration">An object that defines the configuration of this istance.</param>
        /// <param name="sqs">An object that communicates with SQS.</param>
        public SQSQueueSender(ISQSConfiguration configuration, IAmazonSQS sqs)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (sqs == null) throw new ArgumentNullException(nameof(sqs));

            _name = configuration.Name;
            _queueUrl = configuration.QueueUrl;
            _compressed = configuration.Compressed;
            _sqs = sqs;
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="SQSQueueSender"/>.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public Task SendAsync(ISenderMessage message)
        {
            var shouldCompress = message.ShouldCompress(_compressed);

            var stringValue = message.StringValue;

            if (shouldCompress)
            {
                stringValue = MessageCompression.Compress(stringValue);
            }

            var sendMessageRequest = new SendMessageRequest(_queueUrl, stringValue);

            var originatingSystemAlreadyExists = false;

            if (message.Headers != null)
            {
                foreach (var header in message.Headers)
                {
                    if (header.Key == HeaderName.OriginatingSystem)
                    {
                        originatingSystemAlreadyExists = true;
                    }

                    sendMessageRequest.MessageAttributes.Add(
                        header.Key,
                        new MessageAttributeValue { StringValue = header.Value, DataType = "String" });
                }
            }

            sendMessageRequest.MessageAttributes[HeaderName.MessageFormat] =
                new MessageAttributeValue { StringValue = message.MessageFormat.ToString(), DataType = "String" };

            if (!originatingSystemAlreadyExists)
            {
                sendMessageRequest.MessageAttributes[HeaderName.OriginatingSystem] =
                    new MessageAttributeValue { StringValue = "SQS", DataType = "String" };
            }

            if (shouldCompress)
            {
                sendMessageRequest.MessageAttributes[HeaderName.CompressedPayload] =
                    new MessageAttributeValue { StringValue = "true", DataType = "String" };
            }

            return _sqs.SendMessageAsync(sendMessageRequest);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _sqs.Dispose();
        }
    }
}

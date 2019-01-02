using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="SQSReceiver"/>
    /// class.
    /// </summary>
    public sealed class SQSReceiverMessage : ReceiverMessage
    {
        private readonly Func<CancellationToken, Task> _deleteMessageAsync;
        private readonly bool _unpackSns;

        internal SQSReceiverMessage(Message message, Func<CancellationToken, Task> deleteMessageAsync, bool unpackSNS)
            : base(() => ParseSNSBody(message, unpackSNS))
        {
            Message = message;
            _deleteMessageAsync = deleteMessageAsync;
            _unpackSns = unpackSNS;
        }

        /// <summary>
        /// Gets the actual SQS message that was received.
        /// </summary>
        public Message Message { get; }

        /// <inheritdoc />
        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken) => _deleteMessageAsync(cancellationToken);

        /// <inheritdoc />
        protected override Task RollbackMessageAsync(CancellationToken cancellationToken) => Task.FromResult(0); // Do nothing - the message will automatically be redelivered by SQS when left unacknowledged.

        /// <inheritdoc />
        protected override Task RejectMessageAsync(CancellationToken cancellationToken) => _deleteMessageAsync(cancellationToken);

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            if (_unpackSns)
            {
                try
                {
                    var parsedMessage = JsonConvert.DeserializeObject<SNSMessage>(Message.Body);

                    if (parsedMessage.TopicARN != null && parsedMessage.TopicARN.StartsWith("arn:"))
                    {
                        headers["TopicARN"] = parsedMessage.TopicARN;
                        foreach (var attribute in parsedMessage.MessageAttributes)
                            headers[attribute.Key] = attribute.Value.Value;
                        return;
                    }
                }
                catch
                {
                }
            }

            foreach (var attribute in Message.MessageAttributes)
                headers.Add(attribute.Key, attribute.Value.StringValue);
        }

        private static string ParseSNSBody(Message message, bool unpackSNS)
        {
            if (unpackSNS)
            {
                try
                {
                    var parsedMessage = JsonConvert.DeserializeObject<SNSMessage>(message.Body);

                    if (parsedMessage.TopicARN != null && parsedMessage.TopicARN.StartsWith("arn:"))
                    {
                        return parsedMessage.Message;
                    }
                }
                catch
                {
                }
            }

            return message.Body;
        }

        private class SNSMessage
        {
            public string TopicARN { get; set; }
            public string Message { get; set; }
            public Dictionary<string, MessageAttribute> MessageAttributes { get; set; }
        }

        private class MessageAttribute
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}

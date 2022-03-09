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
        private readonly Func<CancellationToken, Task> _rollbackMessageAsync;
        private readonly bool _unpackSns;

        internal SQSReceiverMessage(Message message, Func<CancellationToken, Task> deleteMessageAsync, 
            Func<CancellationToken, Task> rollbackMessageAsync, bool unpackSNS)
            : base(() => GetRawPayload(message.Body, unpackSNS)!)
        {
            Message = message;
            _deleteMessageAsync = deleteMessageAsync;
            _rollbackMessageAsync = rollbackMessageAsync;
            _unpackSns = unpackSNS;
        }

        /// <summary>
        /// Gets the actual SQS message that was received.
        /// </summary>
        public Message Message { get; }

        /// <inheritdoc />
        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken) => _deleteMessageAsync(cancellationToken);

        /// <inheritdoc />
        protected override Task RollbackMessageAsync(CancellationToken cancellationToken) => _rollbackMessageAsync(cancellationToken);

        /// <inheritdoc />
        protected override Task RejectMessageAsync(CancellationToken cancellationToken) => _deleteMessageAsync(cancellationToken);

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            if (headers is null)
            {
                throw new ArgumentNullException(nameof(headers));
            }
            if (TryGetSNSMessage(Message.Body, _unpackSns, out var snsMessage))
            {

                headers["TopicARN"] = snsMessage?.TopicARN!;

                foreach (var attribute in snsMessage?.MessageAttributes!)
                    headers[attribute.Key] = attribute.Value.Value!;
            }
            else
            {
                foreach (var attribute in Message.Attributes)
                    headers[$"SQS.{attribute.Key}"] = attribute.Value;

                foreach (var attribute in Message.MessageAttributes)
                    headers[attribute.Key] = attribute.Value.StringValue;
            }
        }

        private static string? GetRawPayload(string messageBody, bool unpackSNS)
        {
            if (TryGetSNSMessage(messageBody, unpackSNS, out var snsMessage))
            {
                return snsMessage?.Message;
            }

            return messageBody;
        }

        private static bool TryGetSNSMessage(string messageBody, bool unpackSNS, out SNSMessage? snsMessage)
        {
            if (unpackSNS)
            {
                try
                {
                    snsMessage = JsonConvert.DeserializeObject<SNSMessage>(messageBody)!;
                    if (snsMessage.TopicARN is not null && snsMessage.TopicARN.StartsWith("arn:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch { }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            snsMessage = null;
            return false;
        }

        private class SNSMessage
        {
            public string? TopicARN { get; set; }
            public string? Message { get; set; }
            public Dictionary<string, MessageAttribute>? MessageAttributes { get; } = new();
        }

        private class MessageAttribute
        {
            public string? Value { get; set; }
        }
    }
}

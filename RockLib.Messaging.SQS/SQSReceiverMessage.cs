using System;
using System.Collections.Generic;
using Amazon.SQS.Model;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="SQSQueueReceiver"/>
    /// class.
    /// </summary>
    public sealed class SQSReceiverMessage : ReceiverMessage
    {
        private readonly Message _message;
        private readonly Action _deleteMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiverMessage"/> class.
        /// </summary>
        /// <param name="message">The SQS message that was received.</param>
        /// <param name="deleteMessage">
        /// A delegate that deletes the message when invoked.
        /// </param>
        internal SQSReceiverMessage(Message message, Action deleteMessage)
            : base(() => message.Body)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _deleteMessage = deleteMessage ?? throw new ArgumentNullException(nameof(deleteMessage));
        }

        /// <inheritdoc />
        protected override void AcknowledgeMessage() => _deleteMessage();

        /// <inheritdoc />
        protected override void RollbackMessage() { } // Do nothing - the message will automatically be redelivered by SQS when left unacknowledged.

        /// <inheritdoc />
        protected override void RejectMessage() => _deleteMessage();

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            foreach (var attribute in _message.MessageAttributes)
                headers.Add(attribute.Key, attribute.Value.StringValue);
        }
    }
}

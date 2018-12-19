using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly Func<CancellationToken, Task> _deleteMessageAsync;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiverMessage"/> class.
        /// </summary>
        /// <param name="message">The SQS message that was received.</param>
        /// <param name="deleteMessageAsync">
        /// A delegate that deletes the message when invoked.
        /// </param>
        internal SQSReceiverMessage(Message message, Func<CancellationToken, Task> deleteMessageAsync)
            : base(() => message.Body)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _deleteMessageAsync = deleteMessageAsync ?? throw new ArgumentNullException(nameof(deleteMessageAsync));
        }

        /// <inheritdoc />
        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken) => _deleteMessageAsync(cancellationToken);

        /// <inheritdoc />
        protected override Task RollbackMessageAsync(CancellationToken cancellationToken) => Task.FromResult(0); // Do nothing - the message will automatically be redelivered by SQS when left unacknowledged.

        /// <inheritdoc />
        protected override Task RejectMessageAsync(CancellationToken cancellationToken) => _deleteMessageAsync(cancellationToken);

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            foreach (var attribute in _message.MessageAttributes)
                headers.Add(attribute.Key, attribute.Value.StringValue);
        }
    }
}

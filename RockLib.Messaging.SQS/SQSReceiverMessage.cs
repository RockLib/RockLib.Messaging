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
        private readonly Action _acknowledge;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiverMessage"/> class.
        /// </summary>
        /// <param name="message">The SQS message that was received.</param>
        /// <param name="acknowledge">
        /// The <see cref="Action"/> that is invoked when the <see cref="Acknowledge"/> method is called.
        /// </param>
        public SQSReceiverMessage(Message message, Action acknowledge)
            : base(() => message.Body)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _acknowledge = acknowledge ?? throw new ArgumentNullException(nameof(acknowledge));
        }

        /// <summary>
        /// Returns null.
        /// </summary>
        public override byte? Priority => null;

        /// <summary>
        /// Deletes the message from the SQS queue, ensuring it is not redelivered.
        /// </summary>
        public override void Acknowledge() => _acknowledge();

        /// <summary>
        /// Does nothing - the message will automatically be redelivered by SQS if left unacknowledged.
        /// </summary>
        public override void Rollback() { }

        /// <summary>
        /// Deletes the message from the SQS queue, ensuring it is not redelivered.
        /// </summary>
        public override void Reject() => _acknowledge();

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            foreach (var attribute in _message.MessageAttributes)
                headers.Add(attribute.Key, attribute.Value.StringValue);
        }
    }
}

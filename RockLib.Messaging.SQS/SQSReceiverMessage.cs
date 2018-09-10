using System;
using System.Collections.Generic;
using Amazon.SQS.Model;
using RockLib.Messaging.ImplementationHelpers;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="SQSQueueReceiver"/>
    /// class.
    /// </summary>
    public class SQSReceiverMessage : IReceiverMessage
    {
        private readonly Message _message;
        private readonly Action _acknowledge;

        private readonly Lazy<string> _stringPayload;
        private readonly Lazy<byte[]> _binaryPayload;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiverMessage"/> class.
        /// </summary>
        /// <param name="message">The SQS message that was received.</param>
        /// <param name="acknowledge">
        /// The <see cref="Action"/> that is invoked when the <see cref="Acknowledge"/> method is called.
        /// </param>
        public SQSReceiverMessage(Message message, Action acknowledge)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _acknowledge = acknowledge ?? throw new ArgumentNullException(nameof(acknowledge));

            var headers = new Dictionary<string, object>();
            foreach (var attribute in message.MessageAttributes)
                headers.Add(attribute.Key, attribute.Value.StringValue);
            Headers = new HeaderDictionary(headers);

            this.SetLazyPayloadFields(message.Body, out _stringPayload, out _binaryPayload);
        }

        /// <summary>
        /// Gets the payload of the message as a string.
        /// </summary>
        public string StringPayload => _stringPayload.Value;

        /// <summary>
        /// Gets the payload of the message as a byte array.
        /// </summary>
        public byte[] BinaryPayload => _binaryPayload.Value;

        /// <summary>
        /// Gets the headers of the message.
        /// </summary>
        public HeaderDictionary Headers { get; }

        /// <summary>
        /// Returns null.
        /// </summary>
        public byte? Priority => null;

        /// <summary>
        /// Returns true,
        /// </summary>
        public bool IsTransactional => true;

        /// <summary>
        /// Deletes the message from the SQS queue, ensuring it is not redelivered.
        /// </summary>
        public void Acknowledge() => _acknowledge();

        /// <summary>
        /// Does nothing - the message will automatically be redelivered by SQS if left unacknowledged.
        /// </summary>
        public void Rollback() {}
    }
}
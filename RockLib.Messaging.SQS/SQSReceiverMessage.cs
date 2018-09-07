using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public string StringPayload => _stringPayload.Value;

        public byte[] BinaryPayload => _binaryPayload.Value;

        public HeaderDictionary Headers { get; }

        public byte? Priority => null;

        public bool IsTransactional => true;

        public void Acknowledge() => _acknowledge();

        public void Rollback() {} // Nothing to do - SQS will automatically redeliver the message if it is not deleted.
    }
}
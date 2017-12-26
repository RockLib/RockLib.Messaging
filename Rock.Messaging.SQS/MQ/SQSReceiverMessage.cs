using System;
using System.Linq;
using System.Text;
using Amazon.SQS.Model;

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
    /// An implementation of IReceiverMessage for use by the <see cref="SQSQueueReceiver"/>
    /// class.
    /// </summary>
    public class SQSReceiverMessage : IReceiverMessage
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
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (acknowledge == null) throw new ArgumentNullException(nameof(acknowledge));

            _message = message;
            _acknowledge = acknowledge;
        }

        /// <summary>
        /// Gets the priority of the received message. Always returns null.
        /// </summary>
        public byte? Priority { get { return null; } }

        /// <summary>
        /// Gets the actual SQS message that was received.
        /// </summary>
        public Message Message { get { return _message; } }

        /// <summary>
        /// Gets the string value of the message.
        /// </summary>
        /// <param name="encoding">The encoding to use. Ignored.</param>
        /// <returns>The string value of the message.</returns>
        public string GetStringValue(Encoding encoding)
        {
            var stringValue = RawStringValue;

            if (_message.MessageAttributes.ContainsKey(HeaderName.CompressedPayload)
                && _message.MessageAttributes[HeaderName.CompressedPayload].StringValue == "true")
            {
                stringValue = MessageCompression.Decompress(stringValue);
            }

            return stringValue;
        }

        /// <summary>
        /// Gets the binary value of the message by calling the <see cref="GetStringValue"/> method.
        /// If <paramref name="encoding"/> is null, the string value is converted to a byte array
        /// using base 64 encoding. Otherwise, <paramref name="encoding"/>.<see cref="Encoding.GetBytes(string)"/>
        /// is used to convert the string.
        /// </summary>
        /// <param name="encoding">
        /// The encoding to use. A null value indicates that base 64 encoding should be used.
        /// </param>
        /// <returns>The binary value of the message.</returns>
        public byte[] GetBinaryValue(Encoding encoding)
        {
            var stringValue = GetStringValue(encoding);

            return
                stringValue == null
                    ? null
                    : encoding == null
                        ? Convert.FromBase64String(stringValue)
                        : encoding.GetBytes(stringValue);
        }

        /// <summary>
        /// Gets a header value by key.
        /// </summary>
        /// <param name="key">The key of the header to retrieve.</param>
        /// <param name="encoding">The encoding to use. Ignored.</param>
        /// <returns>The string value of the header.</returns>
        public string GetHeaderValue(string key, Encoding encoding)
        {
            return _message.MessageAttributes[key].StringValue;
        }

        /// <summary>
        /// Gets the names of the headers that are available for this message.
        /// </summary>
        /// <returns>An array containing the names of the headers for this message.</returns>
        public string[] GetHeaderNames()
        {
            return _message.MessageAttributes.Keys.ToArray();
        }

        /// <summary>
        /// Acknowledges the message.
        /// </summary>
        public void Acknowledge()
        {
            _acknowledge();
        }

        /// <summary>
        /// Returns an instance of <see cref="StringSenderMessage"/> that is equivalent to this
        /// instance of <see cref="SQSReceiverMessage"/>.
        /// </summary>
        public ISenderMessage ToSenderMessage()
        {
            // If the received message is compressed, then it will already have the compression
            // header, so it will pass it along to the sender message. But we don't want to
            // double-compress the payload, so pass false for the compressed constructor parameter.
            var senderMessage = new StringSenderMessage(RawStringValue, MessageFormat, compressed:false);

            foreach (var attribute in _message.MessageAttributes)
            {
                senderMessage.Headers.Add(attribute.Key, attribute.Value.StringValue);
            }

            return senderMessage;
        }

        private string RawStringValue
        {
            get { return _message.Body; }
        }

        private MessageFormat MessageFormat
        {
            get
            {
                if (_message.MessageAttributes.ContainsKey(HeaderName.MessageFormat))
                {
                    MessageFormat messageFormat;
                    if (Enum.TryParse(_message.MessageAttributes[HeaderName.MessageFormat].StringValue, out messageFormat))
                    {
                        return messageFormat;
                    }
                }

                return MessageFormat.Text;
            }
        }
    }
}
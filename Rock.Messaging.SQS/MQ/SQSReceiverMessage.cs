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
    public class SQSReceiverMessage : IReceiverMessage
    {
        private readonly Message _message;
        private readonly Action _acknowledge;

        public SQSReceiverMessage(Message message, Action acknowledge)
        {
            _message = message;
            _acknowledge = acknowledge;
        }

        public byte? Priority { get { return null; } }

        public Message Message { get { return _message; } }

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

        public string GetHeaderValue(string key, Encoding encoding)
        {
            return _message.MessageAttributes[key].StringValue;
        }

        public string[] GetHeaderNames()
        {
            return _message.MessageAttributes.Keys.ToArray();
        }

        public void Acknowledge()
        {
            _acknowledge();
        }

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
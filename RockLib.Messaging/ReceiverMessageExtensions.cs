using System;

namespace RockLib.Messaging
{
    public static class ReceiverMessageExtensions
    {
        public static string GetMessageId(this IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetStringValue(HeaderNames.MessageId, out var messageId) ? messageId : null;

        public static bool IsCompressed(this IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetBooleanValue(HeaderNames.CompressedPayload, out var isCompressed) && isCompressed;

        public static bool IsBinary(this IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetBooleanValue(HeaderNames.IsBinaryMessage, out var isBinary) && isBinary;

        public static string GetOriginatingSystem(this IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetStringValue(HeaderNames.OriginatingSystem, out var originatingSystem) ? originatingSystem : null;

        public static SenderMessage ToSenderMessage(this IReceiverMessage receiverMessage, Func<object, object> validateHeaderValue = null)
        {
            SenderMessage senderMessage;

            if (receiverMessage.IsBinary())
                senderMessage = new SenderMessage(receiverMessage.BinaryPayload, receiverMessage.Priority, validateHeaderValue: validateHeaderValue);
            else
                senderMessage = new SenderMessage(receiverMessage.StringPayload, receiverMessage.Priority, validateHeaderValue: validateHeaderValue);

            foreach (var header in receiverMessage.Headers)
                senderMessage.Headers[header.Key] = header.Value;

            return senderMessage;
        }
    }
}
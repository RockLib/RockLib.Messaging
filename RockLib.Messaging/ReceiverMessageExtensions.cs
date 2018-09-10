using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines extension methods for implementations of the <see cref="IReceiverMessage"/>
    /// interface.
    /// </summary>
    public static class ReceiverMessageExtensions
    {
        /// <summary>
        /// Gets the ID of the message, or null if not found in the message's headers.
        /// </summary>
        /// <param name="receiverMessage">The source <see cref="IReceiverMessage"/> object.</param>
        /// <returns>The ID of the message.</returns>
        public static string GetMessageId(this IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetStringValue(HeaderNames.MessageId, out var messageId) ? messageId : null;

        /// <summary>
        /// Gets a value indicating whether the message's payload was sent compressed,
        /// as indicated in the message's headers.
        /// </summary>
        /// <param name="receiverMessage">The source <see cref="IReceiverMessage"/> object.</param>
        /// <returns>Whether the message's payload was sent compressed.</returns>
        public static bool IsCompressed(this IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetBooleanValue(HeaderNames.CompressedPayload, out var isCompressed) && isCompressed;

        /// <summary>
        /// Gets a value indicating whether the original message was constructed with
        /// a byte array, as indicated in the message's headers. A value of false
        /// means that the original message was constructed with a string.
        /// </summary>
        /// <param name="receiverMessage">The source <see cref="IReceiverMessage"/> object.</param>
        /// <returns>Whether the original message was constructed with a byte array.</returns>
        public static bool IsBinary(this IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetBooleanValue(HeaderNames.IsBinaryMessage, out var isBinary) && isBinary;

        /// <summary>
        /// Gets the originating system of the message, or null if not found in the
        /// message's headers.
        /// </summary>
        /// <param name="receiverMessage">The source <see cref="IReceiverMessage"/> object.</param>
        /// <returns>The originating system of the message.</returns>
        public static string GetOriginatingSystem(this IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetStringValue(HeaderNames.OriginatingSystem, out var originatingSystem) ? originatingSystem : null;

        /// <summary>
        /// Creates an instance of <see cref="SenderMessage"/> that is equivalent to the
        /// specified <see cref="IReceiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">The source <see cref="IReceiverMessage"/> object.</param>
        /// <param name="validateHeaderValue">
        /// A function that validates header values, returning either the value passed to it
        /// or an equivalent value. If a value is invalid, the function should attempt to
        /// convert it to another type that is valid. If a value cannot be converted, the
        /// function should throw an exception.
        /// </param>
        /// <returns>
        /// A new <see cref="SenderMessage"/> instance that is equivalent to the specified
        /// <paramref name="receiverMessage"/> parameter.
        /// </returns>
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
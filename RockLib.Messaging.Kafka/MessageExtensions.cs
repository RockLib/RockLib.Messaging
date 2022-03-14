using System;
using static RockLib.Messaging.Kafka.Constants;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// Extension methods for <see cref="IReceiverMessage"/> and <see cref="SenderMessage"/>
    /// related to Kafka.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Gets the Key of the Kafka message, as stored in the <see cref="KafkaKeyHeader"/> header
        /// of the <see cref="IReceiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">The <see cref="IReceiverMessage"/>.</param>
        /// <returns>The Key of the Kafka message.</returns>
        public static string? GetKafkaKey(this IReceiverMessage receiverMessage)
        {
            if (receiverMessage is null)
            {
                throw new ArgumentNullException(nameof(receiverMessage));
            }

            if (receiverMessage.Headers.TryGetValue(KafkaKeyHeader, out string? kafkaKey))
            {
                return kafkaKey;
            }

            return null;
        }

        /// <summary>
        /// Sets the Key of the Kafka message, as stored in the <see cref="KafkaKeyHeader"/> header
        /// of the <see cref="SenderMessage"/>.
        /// </summary>
        /// <param name="senderMessage">The <see cref="SenderMessage"/>.</param>
        /// <param name="kafkaKey">The Key of the Kafka message. If <see langword="null"/>, the
        /// Kafka Key header of the message is removed.
        /// event is removed.</param>
        public static void SetKafkaKey(this SenderMessage senderMessage, string kafkaKey)
        {
            if (senderMessage is null)
            {
                throw new ArgumentNullException(nameof(senderMessage));
            }

            if (!string.IsNullOrEmpty(kafkaKey))
            {
                senderMessage.Headers[KafkaKeyHeader] = kafkaKey;
            }
            else
            {
                senderMessage.Headers.Remove(KafkaKeyHeader);
            }
        }
    }
}

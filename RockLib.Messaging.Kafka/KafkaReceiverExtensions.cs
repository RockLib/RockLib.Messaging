using Microsoft.CSharp.RuntimeBinder;
using RockLib.Reflection.Optimized;
using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// Extension methods for <see cref="IReceiver"/> where it is a <see cref="KafkaReceiver"/>
    /// or a decorator for a <see cref="KafkaReceiver"/>.
    /// </summary>
    public static class KafkaReceiverExtensions
    {
        /// <summary>
        /// Seeks to the specified timestamp.
        /// </summary>
        /// <param name="receiver">
        /// A <see cref="KafkaReceiver"/> or a decorator for a <see cref="KafkaReceiver"/>.
        /// </param>
        /// <param name="timestamp">The timestamp to seek to.</param>
        /// <exception cref="InvalidOperationException">
        /// If the <see cref="KafkaReceiver.Consumer"/> has not yet been assigned.
        /// </exception>
        /// <exception cref="RuntimeBinderException">
        /// If <paramref name="receiver"/> is not a <see cref="KafkaReceiver"/>, or not a decorator
        /// for a <see cref="KafkaReceiver"/>.
        /// </exception>
        public static void Seek(this IReceiver receiver, DateTime timestamp)
        {
            dynamic r = receiver.Undecorate();
            r.Seek(timestamp);
        }

        /// <summary>
        /// Replays messages that were created from <paramref name="start"/> to <paramref name=
        /// "end"/>, invoking the <paramref name="callback"/> delegate for each message. If
        /// <paramref name="end"/> is null, then messages that were created from <paramref name=
        /// "start"/> to the current UTC time are replayed.
        /// </summary>
        /// <param name="receiver">
        /// A <see cref="KafkaReceiver"/> or a decorator for a <see cref="KafkaReceiver"/>.
        /// </param>
        /// <param name="start">The start time.</param>
        /// <param name="end">
        /// The end time, or <see langword="null"/> to use the current time as the end time.
        /// </param>
        /// <param name="callback">
        /// The delegate to invoke for each replayed message, or <see langword="null"/> to replay
        /// messages using <see cref="Receiver.MessageHandler"/>.
        /// </param>
        /// <param name="pauseDuringReplay">
        /// Whether to pause the consumer while replaying, then resume after replaying is finished.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the receiver has not been started yet and <paramref name="callback"/> is null.
        /// </exception>
        /// <exception cref="RuntimeBinderException">
        /// If <paramref name="receiver"/> is not a <see cref="KafkaReceiver"/>, or not a decorator
        /// for a <see cref="KafkaReceiver"/>.
        /// </exception>
        public static Task ReplayAsync(this IReceiver receiver, DateTime start, DateTime? end,
            Func<IReceiverMessage, Task> callback = null, bool pauseDuringReplay = false)
        {
            dynamic r = receiver.Undecorate();
            return r.ReplayAsync(start, end, callback, pauseDuringReplay);
        }
    }
}

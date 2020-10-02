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
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="receiver"/> is not a kafka receiver or a decorator for a kafka
        /// receiver.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the <see cref="KafkaReceiver.Consumer"/> has not yet been assigned.
        /// </exception>
        public static void Seek(this IReceiver receiver, DateTime timestamp)
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));

            receiver.AsKafkaReceiver().Seek(timestamp);
        }

        /// <summary>
        /// Pauses consumption of the stream.
        /// </summary>
        /// <param name="receiver">
        /// A <see cref="KafkaReceiver"/> or a decorator for a <see cref="KafkaReceiver"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="receiver"/> is not a kafka receiver or a decorator for a kafka
        /// receiver.
        /// </exception>
        public static void Pause(this IReceiver receiver)
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));

            receiver.AsKafkaReceiver().Pause();
        }

        /// <summary>
        /// Resumes consumption of the stream.
        /// </summary>
        /// <param name="receiver">
        /// A <see cref="KafkaReceiver"/> or a decorator for a <see cref="KafkaReceiver"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="receiver"/> is not a kafka receiver or a decorator for a kafka
        /// receiver.
        /// </exception>
        public static void Resume(this IReceiver receiver)
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));

            receiver.AsKafkaReceiver().Resume();
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
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="receiver"/> is not a kafka receiver or a decorator for a kafka
        /// receiver.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="callback"/> is null and the receiver has not been started yet.
        /// </exception>
        public static Task ReplayAsync(this IReceiver receiver, DateTime start, DateTime? end,
            Func<IReceiverMessage, Task> callback = null, bool pauseDuringReplay = false)
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));

            return receiver.AsKafkaReceiver().ReplayAsync(start, end, callback, pauseDuringReplay);
        }

        /// <summary>
        /// Start listening for messages, starting at the specified time, and handle them using
        /// the specified message handler.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="messageHandler">The object that handles received messages.</param>
        /// <param name="startTimestamp">
        /// The timestamp of the stream at which to start listening.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="receiver"/> is not a kafka receiver or a decorator for a kafka
        /// receiver.
        /// </exception>
        public static void Start(this IReceiver receiver,
            IMessageHandler messageHandler, DateTime startTimestamp)
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));
            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));

            receiver.AsKafkaReceiver().StartTimestamp = startTimestamp;
            receiver.Start(messageHandler);
        }

        /// <summary>
        /// Start listening for messages, starting at the specified time, and handle them using
        /// the specified callback function.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onMessageReceivedAsync">
        /// A function that is invoked when a message is received.
        /// </param>
        /// <param name="startTimestamp">
        /// The timestamp of the stream at which to start listening.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="receiver"/> is not a kafka receiver or a decorator for a kafka
        /// receiver.
        /// </exception>
        public static void Start(this IReceiver receiver,
            OnMessageReceivedAsyncDelegate onMessageReceivedAsync, DateTime startTimestamp)
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));
            if (onMessageReceivedAsync is null)
                throw new ArgumentNullException(nameof(onMessageReceivedAsync));

            receiver.AsKafkaReceiver().StartTimestamp = startTimestamp;
            receiver.Start(onMessageReceivedAsync);
        }

        /// <summary>
        /// Start listening for messages, starting at the specified time, and handle them using
        /// the specified callback function.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onMessageReceivedAsync">
        /// A function that is invoked when a message is received.
        /// </param>
        /// <param name="startTimestamp">
        /// The timestamp of the stream at which to start listening.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> or <paramref name="onMessageReceivedAsync"/> is <see
        /// langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="receiver"/> is not a kafka receiver or a decorator for a kafka
        /// receiver.
        /// </exception>
        public static void Start(this IReceiver receiver,
            Func<IReceiverMessage, Task> onMessageReceivedAsync, DateTime startTimestamp)
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));
            if (onMessageReceivedAsync is null)
                throw new ArgumentNullException(nameof(onMessageReceivedAsync));

            receiver.AsKafkaReceiver().StartTimestamp = startTimestamp;
            receiver.Start(onMessageReceivedAsync);
        }

        private static IKafkaReceiver AsKafkaReceiver(this IReceiver receiver) =>
            receiver.Undecorate() as IKafkaReceiver
                ?? throw new ArgumentException("Must be a kafka receiver or a decorator for a kafka receiver.", nameof(receiver));
    }
}

using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// Defines an interface for replaying messages.
    /// </summary>
    public interface IReplayEngine
    {
        /// <summary>
        /// Replays messages that were created from <paramref name="start"/> to <paramref name=
        /// "end"/>, invoking the <paramref name="callback"/> delegate for each message. If
        /// <paramref name="end"/> is null, then messages that were created from <paramref name=
        /// "start"/> to the current UTC time are replayed.
        /// </summary>
        /// <param name="start">The start time.</param>
        /// <param name="end">
        /// The end time, or <see langword="null"/> to use the current time as the end time.
        /// </param>
        /// <param name="callback">The delegate to invoke for each replayed message.</param>
        /// <param name="topic">
        /// The topic to subscribe to. A regex can be specified to subscribe to the set of
        /// all matching topics (which is updated as topics are added / removed from the
        /// cluster). A regex must be front anchored to be recognized as a regex. e.g. ^myregex
        /// </param>
        /// <param name="bootstrapServers">
        /// List of brokers as a CSV list of broker host or host:port.
        /// </param>
        /// <param name="enableAutoOffsetStore">
        /// Whether to automatically store offset of each message replayed.
        /// </param>
        /// <param name="autoOffsetReset">
        /// Action to take when there is no initial offset in offset store or the desired
        /// offset is out of range: 'smallest','earliest' - automatically reset the offset
        /// to the smallest offset, 'largest','latest' - automatically reset the offset to
        /// the largest offset, 'error' - trigger an error which is retrieved by consuming
        /// messages and checking 'message->err'.
        /// </param>
        Task Replay(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback,
            string topic, string bootstrapServers, bool enableAutoOffsetStore = false,
            AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest);
    }
}

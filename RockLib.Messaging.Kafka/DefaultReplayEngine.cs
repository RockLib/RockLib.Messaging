using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    using static Constants;
    using static KafkaReceiver;

    /// <summary>
    /// The default implementation of <see cref="IReplayEngine"/>.
    /// </summary>
    public class DefaultReplayEngine : IReplayEngine
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
        /// <exception cref="ArgumentException">
        /// If <paramref name="end"/> is earlier than <paramref name="start"/>, or if <paramref
        /// name="end"/> is null and <paramref name="start"/> is after the current UTC time.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="callback"/> is null, or <paramref name="topic"/> is null or empty,
        /// or <paramref name="bootstrapServers"/> is null or empty.
        /// </exception>
        public async Task Replay(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback,
            string topic, string bootstrapServers, bool enableAutoOffsetStore = false,
            AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest)
        {
            if (end.HasValue)
            {
                if (end.Value.Kind != DateTimeKind.Utc)
                    end = end.Value.ToUniversalTime();
                if (end.Value < start)
                    throw new ArgumentException("Cannot be earlier than 'start' parameter.", nameof(end));
            }
            else
            {
                end = GetUtcNow();
                if (end.Value < start)
                    throw new ArgumentException("Cannot be later than DateTime.UtcNow when 'end' parameter is null.", nameof(start));
            }
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentNullException(nameof(topic), "Cannot be null or empty.");
            if (string.IsNullOrEmpty(bootstrapServers))
                throw new ArgumentNullException(nameof(bootstrapServers), "Cannot be null or empty.");

            var consumer = GetConsumer(bootstrapServers, enableAutoOffsetStore, autoOffsetReset);

            var startTimestamps = GetStartTimestamps(topic, bootstrapServers, start);
            var startOffsets = consumer.OffsetsForTimes(startTimestamps, TimeSpan.FromSeconds(5));

            await Replay(consumer, startOffsets, end.Value, callback, enableAutoOffsetStore);
        }

        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        protected virtual DateTime GetUtcNow() => DateTime.UtcNow;

        /// <summary>
        /// Gets a consumer.
        /// </summary>
        protected virtual IConsumer<string, byte[]> GetConsumer(string bootstrapServers,
            bool enableAutoOffsetStore, AutoOffsetReset autoOffsetReset)
        {
            var config = GetConsumerConfig(ReplayGroupId, bootstrapServers, enableAutoOffsetStore, autoOffsetReset);
            var builder = new ConsumerBuilder<string, byte[]>(config);
            return builder.Build();
        }

        /// <summary>
        /// Gets start timestamps.
        /// </summary>
        protected virtual IEnumerable<TopicPartitionTimestamp> GetStartTimestamps(string topic, string bootstrapServers, DateTime start)
        {
            var client = GetAdminClient(bootstrapServers);
            var topics = client.GetMetadata(topic, TimeSpan.FromSeconds(5))?.Topics;

            if (topics == null)
                yield break;

            foreach (var topicMetaData in topics)
                if (topicMetaData.Partitions != null && (topicMetaData.Error == null || !topicMetaData.Error.IsError))
                    foreach (var partitionMetaData in topicMetaData.Partitions)
                        if (partitionMetaData.Error == null || !partitionMetaData.Error.IsError)
                            yield return new TopicPartitionTimestamp(topicMetaData.Topic,
                                new Partition(partitionMetaData.PartitionId), new Timestamp(start));
        }

        /// <summary>
        /// Gets an admin client.
        /// </summary>
        protected virtual IAdminClient GetAdminClient(string bootstrapServers)
        {
            var config = new AdminClientConfig() { BootstrapServers = bootstrapServers };
            var builder = new AdminClientBuilder(config);
            return builder.Build();
        }

        /// <summary>
        /// Replays messages.
        /// </summary>
        protected virtual async Task Replay(IConsumer<string, byte[]> consumer,
            List<TopicPartitionOffset> startOffsets, DateTime endTimestamp,
            Func<IReceiverMessage, Task> callback, bool enableAutoOffsetStore)
        {
            consumer.Assign(startOffsets);

            var partitionsFinished = new bool[startOffsets.Count];

            while (true)
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(5));
                if (result is null)
                    return;

                var afterEndTimestamp = false;

                for (int i = 0; i < startOffsets.Count; i++)
                    if (result.TopicPartition == startOffsets[i].TopicPartition
                        && result.Message.Timestamp.UtcDateTime > endTimestamp)
                    {
                        afterEndTimestamp = partitionsFinished[i] = true;
                        break;
                    }

                if (partitionsFinished.All(finished => finished is true))
                    return;

                if (afterEndTimestamp)
                    continue;

                var message = new KafkaReceiverMessage(consumer, result, enableAutoOffsetStore);
                try { await callback(message); }
                catch { /* TODO: Something? */ }
            }
        }
    }
}

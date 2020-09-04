﻿using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    using static KafkaReceiver;

    public class DefaultReplayEngine : IReplayEngine
    {
        public async Task Replay(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback, string topic,
            string bootstrapServers, bool enableAutoOffsetStore, AutoOffsetReset autoOffsetReset)
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
                end = DateTime.UtcNow;
                if (end.Value < start)
                    throw new ArgumentException("Cannot be later than DateTime.UtcNow when 'end' parameter is null.", nameof(start));
            }
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentNullException(nameof(topic), "Cannot be null or empty.");
            if (string.IsNullOrEmpty(bootstrapServers))
                throw new ArgumentNullException(nameof(bootstrapServers), "Cannot be null or empty.");

            var endTimestamp = end.Value;

            var consumer = GetConsumer(bootstrapServers, enableAutoOffsetStore, autoOffsetReset);

            var startTimestamps = GetStartTimestamps(topic, bootstrapServers, start);
            var startOffsets = consumer.OffsetsForTimes(startTimestamps, TimeSpan.FromSeconds(5));

            await Replay(consumer, startOffsets, endTimestamp, callback, enableAutoOffsetStore);
        }

        protected virtual IConsumer<string, byte[]> GetConsumer(string bootstrapServers, bool enableAutoOffsetStore, AutoOffsetReset autoOffsetReset)
        {
            var config = GetConsumerConfig(ReplayGroupId, bootstrapServers, enableAutoOffsetStore, autoOffsetReset);
            var builder = new ConsumerBuilder<string, byte[]>(config);
            return builder.Build();
        }

        protected virtual IEnumerable<TopicPartitionTimestamp> GetStartTimestamps(string topic, string bootstrapServers, DateTime start)
        {
            var config = new AdminClientConfig() { BootstrapServers = bootstrapServers };
            var builder = new AdminClientBuilder(config);
            var client = builder.Build();
            var topics = client.GetMetadata(topic, TimeSpan.FromSeconds(5)).Topics;

            foreach (var topicMetaData in topics)
                foreach (var partition in topicMetaData.Partitions)
                    yield return new TopicPartitionTimestamp(topicMetaData.Topic, new Partition(partition.PartitionId), new Timestamp(start));
        }

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

                for (int i = 0; i < startOffsets.Count; i++)
                    if (result.TopicPartition == startOffsets[i].TopicPartition
                        && result.Message.Timestamp.UtcDateTime > endTimestamp)
                        partitionsFinished[i] = true;

                if (partitionsFinished.All(finished => finished is true))
                    return;

                var message = new KafkaReceiverMessage(consumer, result, enableAutoOffsetStore);
                try { await callback(message); }
                catch { /* TODO: Something? */ }
            }
        }
    }
}
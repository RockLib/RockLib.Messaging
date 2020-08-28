using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that receives messages from Kafka.
    /// </summary>
    public class KafkaReceiver : Receiver
    {
        private readonly Lazy<Thread> _pollingThread;
        private readonly Lazy<IConsumer<string, byte[]>> _consumer;
        private readonly CancellationTokenSource _disposeSource = new CancellationTokenSource();
        private readonly BlockingCollection<Task> _trackingCollection = new BlockingCollection<Task>();
        private readonly Lazy<Thread> _trackingThread;

        private readonly bool _enableAutoOffsetStore;

        private readonly ConsumerConfig _config;

        private bool _stopped;
        private bool _disposed;
        private bool? _connected;

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="topic">
        /// The topic to subscribe to. A regex can be specified to subscribe to the set of
        /// all matching topics (which is updated as topics are added / removed from the
        /// cluster). A regex must be front anchored to be recognized as a regex. e.g. ^myregex
        /// </param>
        /// <param name="groupId">Client group id string. All clients sharing the same group.id belong to the same group.</param>
        /// <param name="bootstrapServers">List of brokers as a CSV list of broker host or host:port.</param>
        /// <param name="enableAutoOffsetStore">
        /// Whether to automatically store offset of last message provided to application.
        /// </param>
        /// <param name="autoOffsetReset">
        /// Action to take when there is no initial offset in offset store or the desired
        /// offset is out of range: 'smallest','earliest' - automatically reset the offset
        /// to the smallest offset, 'largest','latest' - automatically reset the offset to
        /// the largest offset, 'error' - trigger an error which is retrieved by consuming
        /// messages and checking 'message->err'.
        /// </param>
        public KafkaReceiver(string name, string topic, string groupId, string bootstrapServers,
            bool enableAutoOffsetStore = false, AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest)
            : base(name)
        {
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));

            _config = new ConsumerConfig()
            {
                GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId)),
                BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers)),
                EnableAutoOffsetStore = enableAutoOffsetStore,
                AutoOffsetReset = autoOffsetReset
            };

            var consumerBuilder = new ConsumerBuilder<string, byte[]>(_config);
            consumerBuilder.SetErrorHandler(OnError);

            _consumer = new Lazy<IConsumer<string, byte[]>>(() => consumerBuilder.Build());

            _pollingThread = new Lazy<Thread>(() => new Thread(PollForMessages) { IsBackground = true });
            _trackingThread = new Lazy<Thread>(() => new Thread(TrackMessageHandling) { IsBackground = true });

            _enableAutoOffsetStore = enableAutoOffsetStore;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="topic">
        /// The topic to subscribe to. A regex can be specified to subscribe to the set of
        /// all matching topics (which is updated as topics are added / removed from the
        /// cluster). A regex must be front anchored to be recognized as a regex. e.g. ^myregex
        /// </param>
        /// <param name="consumer">The Kafka <see cref="IConsumer{TKey, TValue}" /> to use for receiving messages.</param>
        public KafkaReceiver(string name, string topic, IConsumer<string, byte[]> consumer)
            : base(name)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer));

            Topic = topic ?? throw new ArgumentNullException(nameof(topic));

            _consumer = new Lazy<IConsumer<string, byte[]>>(() => consumer);

            _pollingThread = new Lazy<Thread>(() => new Thread(PollForMessages) { IsBackground = true });
            _trackingThread = new Lazy<Thread>(() => new Thread(TrackMessageHandling) { IsBackground = true });
        }

        /// <summary>
        /// Gets the topic to subscribe to.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Gets the <see cref="IConsumer{TKey, TValue}" /> for this instance of <see cref="KafkaReceiver"/>.
        /// </summary>
        public IConsumer<string, byte[]> Consumer { get { return _consumer.Value; } }

        /// <summary>
        /// Starts the background threads and subscribes to the topic.
        /// </summary>
        protected override void Start()
        {
            _trackingThread.Value.Start();
            _consumer.Value.Subscribe(Topic);
            _pollingThread.Value.Start();
        }

        public void Seek(DateTime timestamp)
        {
            if (!_consumer.IsValueCreated)
                throw new InvalidOperationException();

            var offsets = Consumer.OffsetsForTimes(Consumer.Assignment.Select(tp => new TopicPartitionTimestamp(tp, new Timestamp(timestamp))), TimeSpan.FromSeconds(5));
            foreach (var offset in offsets)
                Consumer.Seek(offset);
        }

        public void Replay(DateTime start, DateTime end, Func<IReceiverMessage, Task> callback)
        {
            Config config = new AdminClientConfig()
            {
                BootstrapServers = _config.BootstrapServers
            };

            var builder = new AdminClientBuilder(config);
            var client = builder.Build();
            var meta = client.GetMetadata(Topic, TimeSpan.FromSeconds(5));

            var topicPartitions = new List<TopicPartition>();

            foreach (var topic in meta.Topics)
            {
                foreach (var partition in topic.Partitions)
                {
                    topicPartitions.Add(new TopicPartition(topic.Topic, new Partition(partition.PartitionId)));
                }
            }

            config = new ConsumerConfig()
            {
                GroupId = "kafka-receiver-replay",
                BootstrapServers = _config.BootstrapServers,
                EnableAutoOffsetStore = _enableAutoOffsetStore
            };
            var consumerBuilder = new ConsumerBuilder<string, byte[]>(config);
            var consumer = consumerBuilder.Build();

            var startTimestamps = topicPartitions.Select(tp => new TopicPartitionTimestamp(tp, new Timestamp(start)));
            var endTimestamps = topicPartitions.Select(tp => new TopicPartitionTimestamp(tp, new Timestamp(end)));

            var startOffsets = consumer.OffsetsForTimes(startTimestamps, TimeSpan.FromSeconds(5));
            var endOffsets = consumer.OffsetsForTimes(endTimestamps, TimeSpan.FromSeconds(5));

            consumer.Assign(startOffsets);

            var partitionsFinished = new bool[endOffsets.Count];

            while (true)
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(5));
                if (result is null)
                    return;

                var message = new KafkaReceiverMessage(consumer, result, _enableAutoOffsetStore);
                callback(message).Wait();

                for (int i = 0; i < endOffsets.Count; i++)
                {
                    if (result.Offset.Value == endOffsets[i].Offset.Value
                        && result.Partition.Value == endOffsets[i].Partition.Value)
                        partitionsFinished[i] = true;
                }

                if (partitionsFinished.All(x => x))
                    return;
            }
        }

        /// <summary>
        /// Stops polling for messages, waits for current messages to be handled, then
        /// closes and disposes the consumer.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            _stopped = true;
            _disposeSource.Cancel();

            if (_pollingThread.IsValueCreated)
                _pollingThread.Value.Join();

            if (_trackingThread.IsValueCreated)
                _trackingThread.Value.Join();

            if (_consumer.IsValueCreated)
            {
                _consumer.Value.Close();
                _consumer.Value.Dispose();
            }

            base.Dispose(disposing);
        }

        private void PollForMessages()
        {
            while (!_stopped)
            {
                try
                {
                    var result = _consumer.Value.Consume(_disposeSource.Token);
                    var message = new KafkaReceiverMessage(_consumer.Value, result, _enableAutoOffsetStore);

                    if (_connected != true)
                    {
                        _connected = true;
                        OnConnected();
                    }

                    var task = MessageHandler.OnMessageReceivedAsync(this, message);
                    _trackingCollection.Add(task);
                }
                catch (OperationCanceledException) when (_disposeSource.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    OnError("Error in polling loop.", ex);
                }
            }
        }

        private void TrackMessageHandling()
        {
            foreach (var task in _trackingCollection)
            {
                try
                {
                    task.GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    OnError("Error while tracking message handler task.", ex);
                }
            }
        }

        private void OnError(IConsumer<string, byte[]> consumer, Error error)
        {
            OnError(error.Reason, new KafkaException(error) { Data = { ["kafka_consumer"] = consumer } });

            if (_connected != false)
            {
                OnDisconnected(error.Reason);
                _connected = false;
            }
        }
    }
}

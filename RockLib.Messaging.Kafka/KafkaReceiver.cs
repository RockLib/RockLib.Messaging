using Confluent.Kafka;
using System;
using System.Collections.Concurrent;
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
        private static readonly Lazy<DefaultReplayEngine> _defaultReplayEngine = new Lazy<DefaultReplayEngine>();

        private readonly Lazy<Thread> _pollingThread;
        private readonly Lazy<IConsumer<string, byte[]>> _consumer;
        private readonly CancellationTokenSource _disposeSource = new CancellationTokenSource();
        private readonly BlockingCollection<Task> _trackingCollection = new BlockingCollection<Task>();
        private readonly Lazy<Thread> _trackingThread;

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
        /// <param name="groupId">
        /// Client group id string. All clients sharing the same group.id belong to the same group.
        /// </param>
        /// <param name="bootstrapServers">
        /// List of brokers as a CSV list of broker host or host:port.
        /// </param>
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
        /// <param name="replayEngine">
        /// The <see cref="IReplayEngine"/> used to replay messages. If <see langword="null"/>,
        /// then a <see cref="DefaultReplayEngine"/> is used.
        /// </param>
        public KafkaReceiver(string name, string topic, string groupId, string bootstrapServers,
            bool enableAutoOffsetStore = false, AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest,
            IReplayEngine replayEngine = null)
            : base(name)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentNullException(nameof(topic));
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentNullException(nameof(groupId));
            if (string.IsNullOrEmpty(bootstrapServers))
                throw new ArgumentNullException(nameof(bootstrapServers));

            Topic = topic;
            GroupId = groupId;
            BootstrapServers = bootstrapServers;
            EnableAutoOffsetStore = enableAutoOffsetStore;
            AutoOffsetReset = autoOffsetReset;
            ReplayEngine = replayEngine ?? _defaultReplayEngine.Value;

            var config = GetConsumerConfig(GroupId, BootstrapServers, EnableAutoOffsetStore, AutoOffsetReset);
            var builder = new ConsumerBuilder<string, byte[]>(config);
            builder.SetErrorHandler(OnError);

            _consumer = new Lazy<IConsumer<string, byte[]>>(() => builder.Build());

            _pollingThread = new Lazy<Thread>(() => new Thread(PollForMessages) { IsBackground = true });
            _trackingThread = new Lazy<Thread>(() => new Thread(TrackMessageHandling) { IsBackground = true });
        }

        /// <summary>
        /// Gets the topic to subscribe to.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Client group id string. All clients sharing the same group.id belong to the same group.
        /// </summary>
        public string GroupId { get; }

        /// <summary>
        /// List of brokers as a CSV list of broker host or host:port.
        /// </summary>
        public string BootstrapServers { get; }

        /// <summary>
        /// Whether to automatically store offset of last message provided to application.
        /// </summary>
        public bool EnableAutoOffsetStore { get; }

        /// <summary>
        /// Action to take when there is no initial offset in offset store or the desired
        /// offset is out of range: 'smallest','earliest' - automatically reset the offset
        /// to the smallest offset, 'largest','latest' - automatically reset the offset to
        /// the largest offset, 'error' - trigger an error which is retrieved by consuming
        /// messages and checking 'message->err'.
        /// </summary>
        public AutoOffsetReset AutoOffsetReset { get; }

        /// <summary>
        /// The <see cref="IReplayEngine"/> used to replay messages.
        /// </summary>
        public IReplayEngine ReplayEngine { get; }

        /// <summary>
        /// Gets the <see cref="IConsumer{TKey, TValue}" /> for this instance of <see cref="KafkaReceiver"/>.
        /// </summary>
        public IConsumer<string, byte[]> Consumer { get { return _consumer.Value; } }

        /// <summary>
        /// Seeks to the specified timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp to seek to.</param>
        /// <exception cref="InvalidOperationException">
        /// If <see cref="Consumer"/> has not yet been assigned.
        /// </exception>
        public void Seek(DateTime timestamp)
        {
            if (!Consumer.Assignment.Any())
                throw new InvalidOperationException("Seek cannot be called before the receiver has been started.");

            var timestamps = Consumer.Assignment.Select(topicPartition => new TopicPartitionTimestamp(topicPartition, new Timestamp(timestamp)));
            var offsets = Consumer.OffsetsForTimes(timestamps, TimeSpan.FromSeconds(5));
            foreach (var offset in offsets)
                Consumer.Seek(offset);
        }

        /// <summary>
        /// Replays messages that were created from <paramref name="start"/> to <paramref name=
        /// "end"/>, invoking the <paramref name="callback"/> delegate for each message. If
        /// <paramref name="end"/> is null, then messages that were created from <paramref name=
        /// "start"/> to the current UTC time are replayed.
        /// </summary>
        /// <param name="start">The start time.</param>
        /// <param name="end">
        /// The end time, or <see langword="null"/> to indicate that the the current UTC time
        /// should be used.
        /// </param>
        /// <param name="callback">
        /// The delegate to invoke for each replayed message, or <see langword="null"/> to indicate
        /// that <see cref="Receiver.MessageHandler"/> should handle replayed messages.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If the receiver has not been started yet and <paramref name="callback"/> is null.
        /// </exception>
        public Task Replay(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback = null)
        {
            if (callback is null)
            {
                if (MessageHandler != null)
                    callback = message => MessageHandler.OnMessageReceivedAsync(this, message);
                else
                    throw new InvalidOperationException($"Replay cannot be called with a null '{nameof(callback)}' parameter before the receiver has been started.");
            }

            return ReplayEngine.Replay(start, end, callback, Topic, BootstrapServers, EnableAutoOffsetStore, AutoOffsetReset);
        }

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
        public static Task Replay(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback,
            string topic, string bootstrapServers, bool enableAutoOffsetStore = false,
            AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest) =>
            _defaultReplayEngine.Value.Replay(start, end, callback, topic, bootstrapServers, enableAutoOffsetStore, autoOffsetReset);

        /// <summary>
        /// Starts the background threads and subscribes to the topic.
        /// </summary>
        protected override void Start()
        {
            _trackingThread.Value.Start();
            _consumer.Value.Subscribe(Topic);
            _pollingThread.Value.Start();
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
                    var message = new KafkaReceiverMessage(_consumer.Value, result, EnableAutoOffsetStore);

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
                    // TODO: Delay the loop?
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

        internal static ConsumerConfig GetConsumerConfig(string groupId, string bootstrapServers, bool enableAutoOffsetStore, AutoOffsetReset autoOffsetReset) =>
            new ConsumerConfig
            {
                GroupId = groupId,
                BootstrapServers = bootstrapServers,
                EnableAutoOffsetStore = enableAutoOffsetStore,
                AutoOffsetReset = autoOffsetReset
            };
    }
}

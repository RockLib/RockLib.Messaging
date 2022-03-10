using Confluent.Kafka;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that receives messages from Kafka.
    /// </summary>
    public class KafkaReceiver : Receiver
    {
        private Task? _kafkaPolling;
        private readonly Lazy<IConsumer<string, byte[]>> _consumer;
        private readonly CancellationTokenSource _consumerCancellation = new();
        private readonly BlockingCollection<KafkaReceiverMessage>? _trackingCollection;
        private Task? _tracking;

        private readonly bool _schemaIdRequired;
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
        /// <param name="schemaIdRequired">Whether the Kafka receiver expects schema information to be present.
        /// When true the first 5 bytes are expected to contain the schema ID according
        /// to the Confluent
        /// <a href="https://docs.confluent.io/platform/current/schema-registry/serdes-develop/index.html#wire-format">wire format</a>.
        /// </param>
        /// <param name="statisticsIntervalMs">
        /// The statistics emit interval in milliseconds. Granularity is 1,000ms. An event handler must be attached to the
        /// <see cref="StatisticsEmitted"/> event to receive the statistics data. Setting to 0 disables statistics. 
        /// </param>
        public KafkaReceiver(string name, string topic, string groupId, string bootstrapServers,
            bool enableAutoOffsetStore = false, AutoOffsetReset autoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest,
            bool schemaIdRequired = false, int statisticsIntervalMs = 0)
            : base(name)
        {
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
            BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
            EnableAutoOffsetStore = enableAutoOffsetStore;
            AutoOffsetReset = autoOffsetReset;

            var config = GetConsumerConfig(groupId, bootstrapServers, enableAutoOffsetStore, autoOffsetReset, statisticsIntervalMs);
            var builder = new ConsumerBuilder<string, byte[]>(config);
            builder.SetErrorHandler(OnError);
            builder.SetStatisticsHandler(OnStatisticsEmitted);

            _consumer = new Lazy<IConsumer<string, byte[]>>(() => builder.Build());
            _trackingCollection = new BlockingCollection<KafkaReceiverMessage>();

            _schemaIdRequired = schemaIdRequired;
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
        /// <param name="consumerConfig">The configuration used in creation of the Kafka consumer.</param>
        /// <param name="schemaIdRequired">Whether the Kafka receiver expects schema information to be present.
        /// When true the first 5 bytes are expected to contain the schema ID according
        /// to the Confluent
        /// <a href="https://docs.confluent.io/platform/current/schema-registry/serdes-develop/index.html#wire-format">wire format</a>
        /// </param>
        public KafkaReceiver(string name, string topic, ConsumerConfig consumerConfig, bool schemaIdRequired = false)
            : base(name)
        {
            if (consumerConfig is null)
            {
                throw new ArgumentNullException(nameof(consumerConfig));
            }

            if (consumerConfig.EnableAutoCommit is false)
            {
                throw new ArgumentOutOfRangeException(nameof(consumerConfig), "The 'EnableAutoCommit' setting must be true.");
            }

            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            GroupId = consumerConfig.GroupId;
            BootstrapServers = consumerConfig.BootstrapServers;
            EnableAutoOffsetStore = consumerConfig.EnableAutoOffsetStore;
            AutoOffsetReset = consumerConfig.AutoOffsetReset;

            var builder = new ConsumerBuilder<string, byte[]>(consumerConfig);
            builder.SetErrorHandler(OnError);
            builder.SetStatisticsHandler(OnStatisticsEmitted);

            _consumer = new Lazy<IConsumer<string, byte[]>>(() => builder.Build());
            _trackingCollection = new BlockingCollection<KafkaReceiverMessage>();

            _schemaIdRequired = schemaIdRequired;
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
        public bool? EnableAutoOffsetStore { get; }

        /// <summary>
        /// Action to take when there is no initial offset in offset store or the desired
        /// offset is out of range: 'smallest','earliest' - automatically reset the offset
        /// to the smallest offset, 'largest','latest' - automatically reset the offset to
        /// the largest offset, 'error' - trigger an error which is retrieved by consuming
        /// messages and checking 'message->err'.
        /// </summary>
        public AutoOffsetReset? AutoOffsetReset { get; }

        /// <summary>
        /// Gets the <see cref="IConsumer{TKey, TValue}" /> for this instance of <see cref="KafkaReceiver"/>.
        /// </summary>
        public IConsumer<string, byte[]> Consumer => _consumer.Value;

        /// <summary>
        /// Occurs when the Kafka consumer emits statistics. The statistics is a JSON formatted string as defined here:
        /// <a href="https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md">https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md</a>
        /// </summary>
#pragma warning disable CA1003 // Use generic event handler instances
        public event EventHandler<string>? StatisticsEmitted;
#pragma warning restore CA1003 // Use generic event handler instances

        /// <summary>
        /// Starts the background threads and subscribes to the topic.
        /// </summary>
        protected override void Start()
        {
            _tracking = Task.Factory.StartNew(async () => await TrackMessageHandlingAsync().ConfigureAwait(false),
                _consumerCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _consumer.Value.Subscribe(Topic);
            _kafkaPolling = Task.Factory.StartNew(() => PollForMessages(),
                _consumerCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Stops polling for messages, waits for current messages to be handled, then
        /// closes and disposes the consumer.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _stopped = true;

            if(_connected is true)
            {
                _consumerCancellation.Cancel();

                _trackingCollection!.CompleteAdding();
                _trackingCollection.Dispose();

                _consumerCancellation.Dispose();

                if (_kafkaPolling?.IsCompleted ?? false)
                {
                    _kafkaPolling?.Dispose();
                }

                if (_tracking?.IsCompleted ?? false)
                {
                    _tracking?.Dispose();
                }
            }

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
                    var result = _consumer.Value.Consume(_consumerCancellation.Token);
                    var message = new KafkaReceiverMessage(_consumer.Value, result, EnableAutoOffsetStore ?? false, _schemaIdRequired);

                    if (_connected != true)
                    {
                        _connected = true;
                        OnConnected();
                    }

                    _trackingCollection!.Add(message);
                }
                catch (OperationCanceledException) when (_consumerCancellation.IsCancellationRequested)
                {
                    return;
                }
                // We don't want to break out of the while loop with an exception,
                // hence the "catch all"
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    OnError("Error in polling loop.", ex);
                }
            }
        }

        private async Task TrackMessageHandlingAsync()
        {
            foreach (var message in _trackingCollection!.GetConsumingEnumerable(_consumerCancellation.Token))
            {
                try
                {
                    await MessageHandler.OnMessageReceivedAsync(this, message).ConfigureAwait(false);
                }
                // We don't want to break out of the while loop with an exception,
                // hence the "catch all"
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
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

        internal static ConsumerConfig GetConsumerConfig(string groupId, string bootstrapServers,
            bool enableAutoOffsetStore, AutoOffsetReset autoOffsetReset, int statisticsIntervalMs) =>
            new ConsumerConfig
            {
                GroupId = groupId,
                BootstrapServers = bootstrapServers,
                EnableAutoOffsetStore = enableAutoOffsetStore,
                AutoOffsetReset = autoOffsetReset,
                StatisticsIntervalMs = statisticsIntervalMs
            };

        /// <summary>
        /// Callback for the Kafka consumer. Invokes the <see cref="StatisticsEmitted"/> event.
        /// </summary>
        private void OnStatisticsEmitted(IConsumer<string, byte[]> consumer, string statistics)
        {
            StatisticsEmitted?.Invoke(this, statistics);
        }
    }
}

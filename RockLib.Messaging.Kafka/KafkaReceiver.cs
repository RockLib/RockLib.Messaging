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
        private readonly Lazy<Thread> _pollingThread;
        private readonly Lazy<IConsumer<Ignore, string>> _consumer;
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
        /// <param name="groupId">Client group id string. All clients sharing the same group.id belong to the same group.</param>
        /// <param name="bootstrapServers">List of brokers as a CSV list of broker host or host:port.</param>
        /// <param name="config">
        /// A collection of librdkafka configuration parameters (refer to
        /// https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md) and parameters
        /// specific to this client (refer to: Confluent.Kafka.ConfigPropertyNames).
        /// </param>
        public KafkaReceiver(string name, string topic, string groupId, string bootstrapServers, ConsumerConfig config = null)
            : base(name)
        {
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            Config = config ?? new ConsumerConfig();

            Config.GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
            Config.BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
            Config.EnableAutoCommit = Config.EnableAutoCommit ?? false;

            var consumerBuilder = new ConsumerBuilder<Ignore, string>(Config);
            consumerBuilder.SetErrorHandler(OnError);

            _consumer = new Lazy<IConsumer<Ignore, string>>(() => consumerBuilder.Build());

            _pollingThread = new Lazy<Thread>(() => new Thread(PollForMessages) { IsBackground = true });
            _trackingThread = new Lazy<Thread>(() => new Thread(TrackMessageHandling) { IsBackground = true });
        }

        /// <summary>
        /// Gets the topic to subscribe to.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Gets the configuration that is used to create the <see cref="Consumer{TKey, TValue}"/> for this receiver.
        /// </summary>
        public ConsumerConfig Config { get; }

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
                    var message = new KafkaReceiverMessage(_consumer.Value, result);

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

        private void OnError(IConsumer<Ignore, string> consumer, Error error)
        {
            OnError(error.Reason, new KafkaException(error));

            if (_connected != false)
            {
                OnDisconnected(error.Reason);
                _connected = false;
            }
        }
    }
}

using Confluent.Kafka;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    public class KafkaReceiver : Receiver
    {
        private readonly Lazy<Thread> _pollingThread;
        private readonly Lazy<Consumer<Ignore, string>> _consumer;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly BlockingCollection<Task> _trackingCollection = new BlockingCollection<Task>();
        private readonly Lazy<Thread> _trackingThread;

        private bool _stopped;
        private bool _disposed;

        public KafkaReceiver(string name, string topic, string groupId, string bootstrapServers, ConsumerConfig config = null)
            : base(name)
        {
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            Config = config ?? new ConsumerConfig();
            Config.GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
            Config.BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
            Config.EnableAutoCommit = Config.EnableAutoCommit ?? false;

            var consumerBuilder = new ConsumerBuilder<Ignore, string>(Config);
            _consumer = new Lazy<Consumer<Ignore, string>>(() => consumerBuilder.Build());

            _pollingThread = new Lazy<Thread>(() => new Thread(PollForMessages) { IsBackground = true });
            _trackingThread = new Lazy<Thread>(() => new Thread(TrackMessageHandling) { IsBackground = true });
        }

        public string Topic { get; }
        public ConsumerConfig Config { get; }

        protected override void Start()
        {
            _trackingThread.Value.Start();
            _pollingThread.Value.Start();
            _consumer.Value.Subscribe(Topic);
        }

        /// <summary>
        /// Signals the polling background thread to exit then waits for it to finish.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            _stopped = true;
            _cancellationTokenSource.Cancel();

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
                    var result = _consumer.Value.Consume(_cancellationTokenSource.Token);
                    var message = new KafkaReceiverMessage(_consumer.Value, result);
                    var task = MessageHandler.OnMessageReceivedAsync(this, message);
                    _trackingCollection.Add(task);
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
    }
}

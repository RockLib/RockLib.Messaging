using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RockLib.Configuration.ObjectFactory;

namespace RockLib.Messaging.RabbitMQ
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that uses RabbitMQ.
    /// </summary>
    public sealed class RabbitReceiver : Receiver
    {
        private readonly Lazy<IConnection> _connection;
        private readonly Lazy<IModel> _channel;

        /// <summary>
        /// Initializes a new instances of the <see cref="RabbitReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="RabbitReceiver"/>.</param>
        /// <param name="connection">A factory that will create the RabbitMQ connection.</param>
        /// <param name="queue">
        /// The name of the queue to receive messages from. If <see langword="null"/> and
        /// <paramref name="exchange"/> is *not* <see langword="null"/>, then a non-durable,
        /// exclusive, autodelete queue with a generated name is created, and the generated
        /// name is used as the queue name.
        /// </param>
        /// <param name="exchange">
        /// The name of the exchange to bind the queue to. If <see langword="null"/>, the queue
        /// is not bound to an exchange and the <paramref name="routingKeys"/> parameter (if
        /// provided) is ignored.
        /// </param>
        /// <param name="routingKeys">
        /// The collection of routing keys used to bind the queue and exchange. Ignored if
        /// the <paramref name="exchange"/> parameter is <see langword="null"/>.
        /// </param>
        /// <param name="prefetchCount">
        /// The maximum number of messages that the server will deliver to the channel before
        /// being acknowledged.
        /// </param>
        /// <param name="autoAck">
        /// Whether messages should be received in an already-acknowledged state. If true, messages
        /// cannot be rolled back.
        /// </param>
        public RabbitReceiver(string name,
            [DefaultType(typeof(ConnectionFactory))] IConnectionFactory connection,
            string queue = null, string exchange = null, IReadOnlyCollection<string> routingKeys = null,
            ushort? prefetchCount = null, bool autoAck = false)
            : base(name)
        {
            Queue = queue;
            Exchange = exchange;
            RoutingKeys = routingKeys;
            PrefetchCount = prefetchCount;
            AutoAck = autoAck;

            _connection = new Lazy<IConnection>(() => connection.CreateConnection());

            _channel = new Lazy<IModel>(() =>
            {
                var channel = Connection.CreateModel();

                if (!string.IsNullOrEmpty(Exchange))
                {
                    if (string.IsNullOrEmpty(Queue))
                        Queue = channel.QueueDeclare().QueueName;

                    if (RoutingKeys == null || RoutingKeys.Count == 0)
                        channel.QueueBind(Queue, Exchange, "");
                    else
                        foreach (var routingKey in RoutingKeys)
                            channel.QueueBind(Queue, Exchange, routingKey ?? "");
                }

                return channel;
            });
        }

        /// <summary>
        /// Gets the name of the queue to receive messages from.
        /// </summary>
        public string Queue { get; private set; }

        /// <summary>
        /// Gets the name of the exchange to bind the queue to.
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Gets the collection of routing keys used to bind the queue and exchange.
        /// </summary>
        public IReadOnlyCollection<string> RoutingKeys { get; }

        /// <summary>
        /// Gets the maximum number of messages that the server will deliver to the
        /// channel before being acknowledged.
        /// </summary>
        public ushort? PrefetchCount { get; }

        /// <summary>
        /// Gets a value indicating whether messages should be received in an
        /// already-acknowledged state.
        /// </summary>
        public bool AutoAck { get; }

        /// <summary>
        /// Gets the RabbitMQ connection.
        /// </summary>
        public IConnection Connection => _connection.Value;

        /// <summary>
        /// Gets the RabbitMQ channel.
        /// </summary>
        public IModel Channel => _channel.Value;

        /// <inheritdoc />
        protected override void Start()
        {
            if (PrefetchCount.HasValue)
                Channel.BasicQos(0, PrefetchCount.Value, false);

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += OnReceived;

            Channel.BasicConsume(Queue, AutoAck, consumer);
        }

        private async void OnReceived(object s, BasicDeliverEventArgs e)
        {
            try
            {
                var message = new RabbitReceiverMessage(e, Channel, AutoAck);
                await MessageHandler.OnMessageReceivedAsync(this, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnError("Error in message handler.", ex);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_channel.IsValueCreated)
                Channel.Dispose();

            if (_connection.IsValueCreated)
                Connection.Dispose();
        }
    }
}
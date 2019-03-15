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
        private readonly Lazy<IBasicConsumer> _consumer;

        /// <summary>
        /// Initializes a new instances of the <see cref="RabbitReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="RabbitReceiver"/>.</param>
        /// <param name="connection">A factory that will create the RabbitMQ connection.</param>
        /// <param name="queueName">
        /// The name of the queue to receive messages from. If <see langword="null"/> and
        /// <paramref name="exchange"/> is not <see langword="null"/>, then a non-durable,
        /// exclusive, autodelete queue with a generated name is created.
        /// </param>
        /// <param name="exchange">
        /// The name of the exchange to bind the queue to. If <see langword="null"/>, the queue
        /// is not bound to an exchange.
        /// </param>
        /// <param name="routingKeys">
        /// The collection of routing keys used to bind the queue and exchange.
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
            string queueName = null, string exchange = null, IReadOnlyCollection<string> routingKeys = null,
            ushort prefetchCount = 10, bool autoAck = false)
            : base(name)
        {
            if (queueName == null && exchange == null)
                throw new ArgumentNullException(nameof(queueName), "'queueName' cannot be null if 'exchange' is also null.");

            QueueName = queueName;
            Exchange = exchange;
            RoutingKeys = routingKeys;
            PrefetchCount = prefetchCount;
            AutoAck = autoAck;

            _connection = new Lazy<IConnection>(() => connection.CreateConnection());

            _channel = new Lazy<IModel>(() =>
            {
                var channel = _connection.Value.CreateModel();

                if (Exchange != null)
                {
                    if (QueueName == null)
                        QueueName = channel.QueueDeclare().QueueName;

                    if (RoutingKeys == null || RoutingKeys.Count == 0)
                        channel.QueueBind(QueueName, Exchange, "");
                    else
                        foreach (var routingKey in RoutingKeys)
                            channel.QueueBind(QueueName, Exchange, routingKey ?? "");
                }

                return channel;
            });

            _consumer = new Lazy<IBasicConsumer>(() =>
            {
                var consumer = new EventingBasicConsumer(_channel.Value);
                consumer.Received += OnReceived;
                return consumer;
            });
        }

        /// <summary>
        /// Gets the name of the queue to receive messages from.
        /// </summary>
        public string QueueName { get; private set; }

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
        public ushort PrefetchCount { get; }

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
            _channel.Value.BasicQos(0, PrefetchCount, false);
            _channel.Value.BasicConsume(QueueName, AutoAck, _consumer.Value);
        }

        private async void OnReceived(object s, BasicDeliverEventArgs e)
        {
            try
            {
                var message = new RabbitReceiverMessage(e, _channel.Value, AutoAck);
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
                _channel.Value.Dispose();

            if (_connection.IsValueCreated)
                _connection.Value.Dispose();
        }
    }
}
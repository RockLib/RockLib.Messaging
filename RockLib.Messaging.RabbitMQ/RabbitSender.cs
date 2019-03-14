using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RockLib.Configuration.ObjectFactory;

namespace RockLib.Messaging.RabbitMQ
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that uses RabbitMQ.
    /// </summary>
    public sealed class RabbitSender : ISender
    {
        private static readonly Task CompletedTask = Task.FromResult(0);

        private readonly Lazy<IConnection> _connection;
        private readonly Lazy<IModel> _channel;

        /// <summary>
        /// Initializes a new instances of the <see cref="RabbitSender"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="RabbitSender"/>.</param>
        /// <param name="connection">A factory that will create the RabbitMQ connection.</param>
        /// <param name="exchange">The exchange to use when publishing messages.</param>
        /// <param name="routingKey">The routing key to use when publishing messages.</param>
        public RabbitSender(string name, 
            [DefaultType(typeof(ConnectionFactory))] IConnectionFactory connection,
            string exchange = "", string routingKey = "")
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Exchange = exchange ?? "";
            RoutingKey = routingKey ?? "";

            _connection = new Lazy<IConnection>(() => connection.CreateConnection());
            _channel = new Lazy<IModel>(() => _connection.Value.CreateModel());
        }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests.
        /// </param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "RabbitMQ";

            var props = _channel.Value.CreateBasicProperties();

            props.Headers = message.Headers;

            // TODO: Should we set any properties (e.g. ContentType, ContentEncoding) on props here?
            // TODO: Should we support having a different routing key per message (possibly embedded in Headers)?

            _channel.Value.BasicPublish(Exchange, RoutingKey, props, message.BinaryPayload);

            return CompletedTask;
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="RabbitSender"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the exchange to use when publishing messages.
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Gets the routing key to use when publishing messages.
        /// </summary>
        public string RoutingKey { get; }

        /// <summary>
        /// Gets the RabbitMQ connection.
        /// </summary>
        public IConnection Connection => _connection.Value;

        /// <summary>
        /// Gets the RabbitMQ channel.
        /// </summary>
        public IModel Channel => _channel.Value;

        /// <summary>
        /// Disposes the RabbitMQ channel and connection.
        /// </summary>
        public void Dispose()
        {
            if (_channel.IsValueCreated)
                _channel.Value.Dispose();

            if (_connection.IsValueCreated)
                _connection.Value.Dispose();
        }
    }
}
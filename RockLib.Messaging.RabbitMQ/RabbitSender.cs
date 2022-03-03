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
        private readonly Lazy<IConnection> _connection;
        private readonly Lazy<IModel> _channel;

        /// <summary>
        /// Initializes a new instances of the <see cref="RabbitSender"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="RabbitSender"/>.</param>
        /// <param name="connection">A factory that will create the RabbitMQ connection.</param>
        /// <param name="exchange">The exchange to use when publishing messages.</param>
        /// <param name="routingKey">The routing key to use when publishing messages.</param>
        /// <param name="routingKeyHeaderName">
        /// The name of the header that contains the routing key to use when publishing messages.
        /// Each message sent that has a header with this name will be sent with a routing key of
        /// the header value.
        /// </param>
        /// <param name="persistent">
        /// Whether the RabbitMQ server should save the message to disk upon receipt.
        /// </param>
        public RabbitSender(string name,
            [DefaultType(typeof(ConnectionFactory))] IConnectionFactory connection,
            string? exchange = null, string? routingKey = null, string? routingKeyHeaderName = null, bool persistent = true)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Exchange = exchange ?? "";
            RoutingKey = routingKey ?? "";
            RoutingKeyHeaderName = routingKeyHeaderName;
            Persistent = persistent;

            _connection = new Lazy<IConnection>(() => connection.CreateConnection());
            _channel = new Lazy<IModel>(() => Connection.CreateModel());
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
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.OriginatingSystem is null)
            {
                message.OriginatingSystem = "RabbitMQ";
            }

            var properties = Channel.CreateBasicProperties();

            properties.Headers = message.Headers;

            if (Persistent)
            {
                properties.Persistent = true;
            }

            // TODO: Should we set any properties (e.g. ContentType, ContentEncoding) on properties here?
            // TODO: Should we support having a different routing key per message (possibly embedded in Headers)?

            Channel.BasicPublish(Exchange, GetRoutingKey(message), properties, message.BinaryPayload);

            return Task.CompletedTask;
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
        /// Gets the name of the header that contains the routing key to use when publishing
        /// messages. Each message sent that has a header with this name will be sent with a
        /// routing key of the header value.
        /// </summary>
        public string? RoutingKeyHeaderName { get; }

        /// <summary>
        /// Gets a value indicating whether the RabbitMQ server should save the message to
        /// disk upon receipt.
        /// </summary>
        public bool Persistent { get; }

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
            {
                Channel.Dispose();
            }

            if (_connection.IsValueCreated)
            {
                Connection.Dispose();
            }
        }

        private string GetRoutingKey(SenderMessage message)
        {
            if (RoutingKeyHeaderName is not null && message.Headers.TryGetValue(RoutingKeyHeaderName, out var value))
            {
                switch (value)
                {
                    case string routingKey:
                        return routingKey;
                    case null:
                        break;
                    default:
                        return value.ToString()!;
                }
            }

            return RoutingKey;
        }
    }
}
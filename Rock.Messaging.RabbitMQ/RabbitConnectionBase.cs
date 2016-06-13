using System;
using RabbitMQ.Client;

namespace Rock.Messaging.RabbitMQ
{
    public abstract class RabbitConnectionBase : IDisposable
    {
        protected IConnectionFactory _connectionFactory;
        protected IConnection _connection;

        protected RabbitConnectionBase(IConnectionFactory conn)
        {
            _connectionFactory = conn;
            _connection = _connectionFactory.CreateConnection();
        }

        public virtual void Dispose()
        {
            _connection.Dispose();
        }
    }
}
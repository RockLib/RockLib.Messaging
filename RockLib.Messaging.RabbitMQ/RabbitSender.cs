using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RockLib.Messaging.RabbitMQ
{
    // needed config values for RabbitSender: u/pass, URL, vHost, exchange name, routing key.
    public class RabbitSender : ISender 
    {
        const byte highestPriority = 9;

        private readonly string _routingKey;
        private readonly string _exchange;
        private readonly IConnection _connection;
        private readonly string _name;

        public RabbitSender(IConnectionFactory conn, string exchange, string routingKey, string name)
        {
            _connection = conn.CreateConnection();
            _exchange = exchange;
            _routingKey = routingKey;
            _name = name;
        }
        public Task SendAsync(ISenderMessage message)
        {
            return Task.Run(delegate
            {
                using (var model = _connection.CreateModel())
                {
                    var props = model.CreateBasicProperties();
                    props.Headers = message.Headers.ToDictionary<KeyValuePair<string, string>, string, object>(kvp => kvp.Key, kvp => kvp.Value); // IEnum<KVP<S,S>> in, Dictionary<string, object> out.
                    props.ContentType = message.MessageFormat.ToString();
                    props.Priority =  message.Priority > highestPriority ? highestPriority : (message.Priority ?? 0);
                    // props.ContentEncoding = new NotImplementedException(); // TODO: add this to Rock.Messaging?
                    model.BasicPublish(_exchange, _routingKey, props, message.BinaryValue);
                }
            });
        }

        public string Name { get { return _name; } }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
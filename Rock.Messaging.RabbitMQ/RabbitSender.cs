using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Rock.Messaging.RabbitMQ
{
    // needed config values for RabbitSender: u/pass, URL, vHost, exchange name, routing key.
    public class RabbitSender : RabbitConnectionBase, ISender 
    {
        const byte highestPriority = 9;
        private string _routingKey;
        private string _exchange;

        public RabbitSender(IConnectionFactory conn, string exchange, string routingKey, string name) : base(conn)
        {
            _exchange = exchange;
            _routingKey = routingKey;
            Name = name;
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

        public string Name { get; }
    }
}
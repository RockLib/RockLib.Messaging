using System.Linq;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace Rock.Messaging.RabbitMQ
{
    public class RabbitReceiverMessage : IReceiverMessage
    {
        private BasicDeliverEventArgs _args;
        private IModel _channel;

        public RabbitReceiverMessage(BasicDeliverEventArgs args, IModel channel)
        {
            _args = args;
            _channel = channel;
        }

        public string GetStringValue(Encoding encoding)
        {
            return encoding.GetString(_args.Body);
        }

        public byte[] GetBinaryValue(Encoding encoding)
        {
            return _args.Body;
        }

        public string GetHeaderValue(string key, Encoding encoding)
        {
            return _args.BasicProperties.Headers.ContainsKey(key)
                ? _args.BasicProperties.Headers[key].ToString() //TODO: make this better? Not sure :D
                : null;
        }

        public string[] GetHeaderNames()
        {
            return _args.BasicProperties.Headers.Keys.ToArray();
        }

        public void Acknowledge()
        {
            _channel.BasicAck(_args.DeliveryTag, false);
        }

        public ISenderMessage ToSenderMessage()
        {
            var senderMessage = new BinarySenderMessage(this.GetBinaryValue(), MessageFormat.Binary, priority: Priority);

            foreach (var name in GetHeaderNames())
            {
                senderMessage.Headers.Add(name, GetHeaderValue(name, Encoding.UTF8));
            }

            return senderMessage;
        }
    

        public byte? Priority { get { return _args.BasicProperties.Priority; } }
    }
}
using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Messaging.RabbitMQ
{
    public class RabbitEngine : IReceiver, ISender
    {
        private readonly IRabbitSessionConfiguration _config;
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _consumerModel; // While this is hacky- IModel is not thread safe.. see: https://www.rabbitmq.com/releases/rabbitmq-dotnet-client/v1.7.1/rabbitmq-dotnet-client-1.7.1-user-guide.pdf

        private bool topical;

        public RabbitEngine(IRabbitSessionConfiguration rabbitSessionConfiguration, bool isTopic = false)
        {
            this._config = rabbitSessionConfiguration;
            topical = isTopic;
            Initialize();
        }
        private void Initialize()
        {
            var connectionStrs = _config.QueueUrl.Split(':');
            _connectionFactory = new ConnectionFactory
            {
                UserName = _config.UserName,
                Password = _config.Password,
                HostName = connectionStrs[0],
                Port = int.Parse(connectionStrs[1]),
                VirtualHost = _config.vHost,
            };
            _connection = _connectionFactory.CreateConnection();
            
        }

        public void Dispose()
        {
            _connection.Dispose();
            _consumerModel.Abort();
        }

        #region Receiver
        public void Start(string selector) // TODO: Discuss: RabbitMQ supports getting individual messages- should Rock.Messaging support that?
        {
            _consumerModel = _connection.CreateModel();
            _consumerModel.ExchangeDeclare(_config.Exchange, _config.ExchangeType ?? (topical ? "topic": "direct"), true); // Should perhaps configure this default somehow? Also RabbitMQ Topics might not fulfill the same intent as the abstraction?
            _consumerModel.QueueDeclare(_config.QueueName, true, false, false, null); // TODO: Discuss: Add config settings for these? It's already pretty cluttered...
            if (!string.IsNullOrWhiteSpace(selector))
            {
                _consumerModel.QueueBind(_config.QueueName, _config.Exchange, selector);
            }
            else
            {
                _consumerModel.QueueBind(_config.QueueName, _config.Exchange, _config.RoutingKey ?? string.Empty);
            }
            
            var consumer = new EventingBasicConsumer(_consumerModel);
            consumer.Received += delegate(object sender, BasicDeliverEventArgs args)
            {
                var messageReceived = new MessageReceivedEventArgs(new RabbitReceiverMessage(args, _consumerModel));
                OnMessageReceived(messageReceived);
                if (_config.AutoAcknowledge)
                {
                    _consumerModel.BasicAck(args.DeliveryTag, false);
                }
            };
            _consumerModel.BasicQos(0, _config.MaxRequests, true);
            _consumerModel.BasicConsume(_config.QueueName, false, consumer);
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs args)
        {
            var handler = MessageReceived;

            if (handler != null)
            {
                handler(this, args);
            }
        }
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        #endregion
        public Task SendAsync(ISenderMessage message)
        {
            return Task.Run(delegate
            {
                using (var model = _connection.CreateModel())
                {
                    var props = model.CreateBasicProperties();
                    props.Headers = message.Headers.ToDictionary<KeyValuePair<string, string>, string, object>(kvp => kvp.Key, kvp => kvp.Value); // IEnum<KVP<S,S>> in, Dictionary<string, object> out.
                    props.ContentType = message.MessageFormat.ToString();
                    props.Priority = message.Priority ?? 0;
                    // props.ContentEncoding = new NotImplementedException(); // TODO: add this to Rock.Messaging?
                    model.BasicPublish(_config.Exchange, _config.RoutingKey, props, message.BinaryValue);
                }
            });
        }

        public string Name { get { return _config.Name; } }
    }
}
using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rock.Messaging.RabbitMQ
{
    /* config settings needed for Receiver: U/pass, URL/vHost, QueueName, MaxSimultaneousRequests, AutoAck */

/* Queue binding settings: exchange, exchangeType, queueName, Routing Key, (TODO: durable, exclusive, autoDelete)
   Note: Queue Binding and the Receiver are together because AutoDelete queues will kill themselves off if they've got no listeners/messages. */

    public class RabbitReceiver : IReceiver
    {
        private bool _isTopic;
        private bool _isStarted;
        
        private string _exchange;
        private string _exchangeType;
        private string _queueName;
        private string _routingKey;
        private ushort _maxRequests;
        private bool _autoAck;
        private string _name;

        private IModel _consumerModel;
        private IConnection _connection;


        public RabbitReceiver(IConnectionFactory conn, IRabbitSessionConfiguration config, bool isTopic = false)
        {
            _isTopic = isTopic;

            // Binding to fields to keep _config from getting everywhere.
            _exchange = config.Exchange;
            _exchangeType = config.ExchangeType;
            _queueName = config.QueueName;
            _routingKey = config.RoutingKey;
            _maxRequests = config.MaxRequests;
            _autoAck = config.AutoAcknowledge;
            _name = config.Name;

            _connection = conn.CreateConnection();
        }


        public void Start(string selector)
        {
            if (_isStarted)
            {
                return;
            }
            _isStarted = true;
            _consumerModel = _connection.CreateModel();
            if (!string.IsNullOrWhiteSpace(_exchange))
            {
                _consumerModel.ExchangeDeclare(_exchange, _exchangeType ?? (_isTopic ? "topic" : "direct"), true);
                // Should perhaps configure this default somehow? Also RabbitMQ Topics might not fulfill the same intent as the abstraction?
                _consumerModel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false,
                    arguments: null);
                // TODO: Discuss: Add config settings for these? It's already pretty cluttered...
                if (!string.IsNullOrWhiteSpace(selector))
                {
                    _consumerModel.QueueBind(_queueName, _exchange, selector);
                }
                else
                {
                    _consumerModel.QueueBind(_queueName, _exchange, _routingKey ?? string.Empty);
                }
            }
            var consumer = new EventingBasicConsumer(_consumerModel);
            consumer.Received += delegate(object sender, BasicDeliverEventArgs args)
            {
                var messageReceived = new MessageReceivedEventArgs(new RabbitReceiverMessage(args, _consumerModel));
                OnMessageReceived(messageReceived);
                if (_autoAck)
                {
                    _consumerModel.BasicAck(args.DeliveryTag, false);
                }
            };
            _consumerModel.BasicQos(0, _maxRequests, true);
            _consumerModel.BasicConsume(_queueName, false, consumer);
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

        public void Dispose()
        {
            _consumerModel.Dispose();
            _connection.Dispose();
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
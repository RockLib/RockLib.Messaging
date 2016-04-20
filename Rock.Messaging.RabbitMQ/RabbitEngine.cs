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
        private readonly IRabbitSessionConfiguration config;
        private ConnectionFactory connectionFactory;
        private IConnection connection;
        private List<IModel> consumerModels; // While this is hacky- IModel is not thread safe.. see: https://www.rabbitmq.com/releases/rabbitmq-dotnet-client/v1.7.1/rabbitmq-dotnet-client-1.7.1-user-guide.pdf

        private bool topical;

        public RabbitEngine(IRabbitSessionConfiguration rabbitSessionConfiguration, bool isTopic = false)
        {
            this.config = rabbitSessionConfiguration;
            topical = isTopic;
            consumerModels = new List<IModel>();
            Initialize();
        }
        private void Initialize()
        {
            var connectionStrs = config.QueueUrl.Split(':');
            connectionFactory = new ConnectionFactory
            {
                UserName = config.UserName,
                Password = config.Password,
                HostName = connectionStrs[0],
                Port = int.Parse(connectionStrs[1]),
                VirtualHost = config.vHost,
            };
            connection = connectionFactory.CreateConnection();
            
        }

        public void Dispose()
        {
            connection.Dispose();
            foreach (var m in consumerModels)
            {
                m.Abort();
            }
            
        }

        #region Receiver
        public void Start(string selector) // TODO: Discuss: RabbitMQ supports getting individual messages- should Rock.Messaging support that?
        {
            var model = connection.CreateModel();
            model.ExchangeDeclare(config.Exchange, config.ExchangeType ?? (topical ? "topic": "direct"), true); // Should perhaps configure this default somehow? Also RabbitMQ Topics might not fulfill the same intent as the abstraction?
            model.QueueDeclare(config.QueueName, true, false, false, null); // TODO: Discuss: Add config settings for these? It's already pretty cluttered...
            if (!string.IsNullOrWhiteSpace(selector))
            {
                model.QueueBind(config.QueueName, config.Exchange, selector);
            }
            else
            {
                model.QueueBind(config.QueueName, config.Exchange, config.RoutingKey ?? string.Empty);
            }
            
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += delegate(object sender, BasicDeliverEventArgs args)
            {
                var messageReceived = new MessageReceivedEventArgs(new RabbitReceiverMessage(args, model));
                OnMessageReceived(messageReceived);
                if (config.AutoAcknowledge)
                {
                    model.BasicAck(args.DeliveryTag, false);
                }
            };
            model.BasicQos(0, config.MaxRequests, true);
            model.BasicConsume(config.QueueName, false, consumer);
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
                using (var model = connection.CreateModel())
                {
                    var props = model.CreateBasicProperties();
                    props.Headers = message.Headers.ToDictionary<KeyValuePair<string, string>, string, object>(kvp => kvp.Key, kvp => kvp.Value); // IEnum<KVP<S,S>> in, Dictionary<string, object> out.
                    props.ContentType = message.MessageFormat.ToString();
                    props.Priority = message.Priority ?? 0;
                    // props.ContentEncoding = new NotImplementedException(); // TODO: add this to Rock.Messaging?
                    model.BasicPublish(config.Exchange, config.RoutingKey, props, message.BinaryValue);
                }
            });
        }

        public string Name { get { return config.Name; } }
    }
}
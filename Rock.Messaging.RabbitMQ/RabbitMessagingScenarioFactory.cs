using System;
using RabbitMQ.Client;

namespace Rock.Messaging.RabbitMQ
{
    public class RabbitMessagingScenarioFactory : IMessagingScenarioFactory
    {
        private readonly IRabbitSessionConfigurationProvider _sessionConfigurationProvider;


        public RabbitMessagingScenarioFactory(IRabbitSessionConfigurationProvider sessionConfigurationProvider)
        {
            if (sessionConfigurationProvider == null)
            {
                throw new ArgumentNullException("sessionConfigurationProvider");
            }
            _sessionConfigurationProvider = sessionConfigurationProvider;
        }

        public RabbitMessagingScenarioFactory(XmlDeserializingRabbitSessionConfigurationProvider rabbitSettings)
            : this((IRabbitSessionConfigurationProvider) rabbitSettings)
        {
        }

        public IRabbitSessionConfigurationProvider SessionConfigurationProvider
        {
            get { return _sessionConfigurationProvider;}
        }

        protected virtual IConnectionFactory CreateFactory(IRabbitSessionConfiguration config)
        {
            var connectionStrs = config.ExchangeUrl.Split(':');
            return new ConnectionFactory
            {
                UserName = config.UserName,
                Password = config.Password,
                HostName = connectionStrs[0],
                Port = int.Parse(connectionStrs[1]),
                VirtualHost = config.vHost,
            };
        }

        public ISender CreateQueueProducer(string name)
        {
            var config = SessionConfigurationProvider.GetConfiguration(name);
            return new RabbitSender(CreateFactory(config), config.Exchange, config.RoutingKey, name);
        }

        public IReceiver CreateQueueConsumer(string name)
        {
            var config = SessionConfigurationProvider.GetConfiguration(name);
            return new RabbitReceiver(CreateFactory(config), config);
        }

        public ISender CreateTopicPublisher(string name)
        {
            return CreateQueueProducer(name); // Senders don't care what they're sending to.
        }

        public IReceiver CreateTopicSubscriber(string name)
        {
            var config = SessionConfigurationProvider.GetConfiguration(name);
            return new RabbitReceiver(CreateFactory(config), config, true);
        }

        public bool HasScenario(string name)
        {
            return _sessionConfigurationProvider.HasConfiguration(name);
        }

        public void Dispose()
        {
        }
    }
}
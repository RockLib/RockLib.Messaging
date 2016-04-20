using System;

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

        public ISender CreateQueueProducer(string name)
        {
            return new RabbitEngine(SessionConfigurationProvider.GetConfiguration(name));
        }

        public IReceiver CreateQueueConsumer(string name)
        {
            return new RabbitEngine(SessionConfigurationProvider.GetConfiguration(name));
        }

        public ISender CreateTopicPublisher(string name)
        {
            return new RabbitEngine(SessionConfigurationProvider.GetConfiguration(name), true);
        }

        public IReceiver CreateTopicSubscriber(string name)
        {
            return new RabbitEngine(SessionConfigurationProvider.GetConfiguration(name), true);
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
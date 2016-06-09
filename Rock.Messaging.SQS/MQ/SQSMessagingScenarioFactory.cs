using Amazon.SQS;
using System;

namespace Rock.Messaging.SQS
{
    public class SQSMessagingScenarioFactory : IMessagingScenarioFactory
    {
        private readonly ISQSConfigurationProvider _configurationProvider;

        public SQSMessagingScenarioFactory(ISQSConfigurationProvider configurationProvider)
        {
            if (configurationProvider == null)
            {
                throw new ArgumentNullException("configurationProvider");
            }

            _configurationProvider = configurationProvider;
        }

        public SQSMessagingScenarioFactory(XmlDeserializingSQSConfigurationProvider sqsSettings)
            : this((ISQSConfigurationProvider)sqsSettings)
        {
        }

        public IReceiver CreateQueueConsumer(string name)
        {
            var configuration = _configurationProvider.GetConfiguration(name);
            return new SQSQueueReceiver(configuration, CreateSqsClient());
        }

        public ISender CreateQueueProducer(string name)
        {
            var configuration = _configurationProvider.GetConfiguration(name);
            return new SQSQueueSender(configuration, CreateSqsClient());
        }

        public ISender CreateTopicPublisher(string name)
        {
            throw new NotImplementedException();
        }

        public IReceiver CreateTopicSubscriber(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a value indicating whether a scenario by the given name can be created by this
        /// instance of <see cref="SQSMessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="name">The name of the scenario.</param>
        /// <returns>True, if the scenario can be created. Otherwise, false.</returns>
        public bool HasScenario(string name)
        {
            return _configurationProvider.HasConfiguration(name);
        }

        public void Dispose()
        {
        }

        private static IAmazonSQS CreateSqsClient()
        {
            return new AmazonSQSClient();
        }
    }
}

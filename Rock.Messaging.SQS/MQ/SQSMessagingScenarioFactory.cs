using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.Linq;

#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    public class SQSMessagingScenarioFactory : IMessagingScenarioFactory
    {
        private ISQSConfigurationProvider _configurationProvider;

#if ROCKLIB

        private List<SQSConfiguration> _sqsSettings;

        public SQSMessagingScenarioFactory()
        {
        }

        public SQSMessagingScenarioFactory(ISQSConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
        }

        public List<SQSConfiguration> SQSSettings
        {
            get => _sqsSettings;
            set
            {
                _sqsSettings = value;
                _configurationProvider = new SQSConfigurationProvider
                {
                    Configurations = value.ToArray<ISQSConfiguration>()
                };
            }
        }
#else
        public SQSMessagingScenarioFactory(ISQSConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
        }

        public SQSMessagingScenarioFactory(XmlDeserializingSQSConfigurationProvider sqsSettings)
            : this((ISQSConfigurationProvider)sqsSettings)
        {
        }
#endif

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

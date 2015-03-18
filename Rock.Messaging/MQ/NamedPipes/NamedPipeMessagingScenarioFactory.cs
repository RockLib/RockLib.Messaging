using System.Collections.Generic;
using Rock.Serialization;

namespace Rock.Messaging.NamedPipes
{
    public class NamedPipeMessagingScenarioFactory : IMessagingScenarioFactory
    {
        private readonly INamedPipeConfigProvider _configProvider;
        private readonly ISerializer _serializer;

        public NamedPipeMessagingScenarioFactory(INamedPipeConfigProvider configProvider, ISerializer serializer)
        {
            _configProvider = configProvider;
            _serializer = serializer;
        }

        public IEnumerable<ISender> CreateQueueProducers(string name, int count)
        {
            var config = _configProvider.GetConfig(name);

            for (int i = 0; i < count; i++)
            {
                yield return new NamedPipeQueueProducer(name, config.PipeName, _serializer);
            }
        }

        public IEnumerable<IReceiver> CreateQueueConsumers(string name, int count)
        {
            var config = _configProvider.GetConfig(name);

            for (int i = 0; i < count; i++)
            {
                yield return new NamedPipeQueueConsumer(name, config.PipeName, _serializer);
            }
        }

        public IEnumerable<ISender> CreateTopicPublishers(string name, int count)
        {
            return CreateQueueProducers(name, count); // Why not?
        }

        public IEnumerable<IReceiver> CreateTopicSubscribers(string name, int count)
        {
            return CreateQueueConsumers(name, count); // Why not?
        }
    }
}

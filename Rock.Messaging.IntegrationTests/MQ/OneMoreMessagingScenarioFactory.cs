using System;

namespace Rock.Messaging
{
    public class PermissiveMessagingScenarioFactory : IMessagingScenarioFactory
    {
        public IReceiver CreateQueueConsumer(string name)
        {
            return new TestReceiver(name, GetType());
        }

        public ISender CreateQueueProducer(string name)
        {
            throw new NotImplementedException();
        }

        public ISender CreateTopicPublisher(string name)
        {
            throw new NotImplementedException();
        }

        public IReceiver CreateTopicSubscriber(string name)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool HasScenario(string name)
        {
            return name != "qux";
        }
    }
}

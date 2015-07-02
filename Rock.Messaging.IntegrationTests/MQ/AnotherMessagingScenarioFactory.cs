using System;

namespace Rock.Messaging
{
    public class AnotherMessagingScenarioFactory : IMessagingScenarioFactory
    {
        private readonly bool _boolData;

        public AnotherMessagingScenarioFactory()
            : this(false)
        {
        }

        public AnotherMessagingScenarioFactory(bool boolData = false)
        {
            _boolData = boolData;
        }

        public bool BoolData
        {
            get { return _boolData; }
        }

        public ISender CreateQueueProducer(string name)
        {
            throw new NotImplementedException();
        }

        public IReceiver CreateQueueConsumer(string name)
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

        public bool HasScenario(string name)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
        }
    }
}
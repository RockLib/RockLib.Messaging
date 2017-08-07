using System;

namespace RockLib.Messaging.Example.Core
{
    public class MessagingSettings
    {
        public string PipeName { get; set; }
        public bool Compressed { get; set; }
    }

    public class NamedPipeMessagingScenarioFactory : IMessagingScenarioFactory
    {

        public MessagingSettings[] MessagingSettings { get; set; }

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
            return name == "foo" || name == "bar";
        }

        void IDisposable.Dispose()
        {
        }
    }
}

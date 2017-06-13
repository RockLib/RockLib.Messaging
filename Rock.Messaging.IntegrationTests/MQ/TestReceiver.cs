using System;

namespace Rock.Messaging
{
    public class TestReceiver : IReceiver
    {
        public TestReceiver(string name, Type factoryType)
        {
            Name = name;
            FactoryType = factoryType;
        }

        public string Name { get; }
        public Type FactoryType { get; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Dispose()
        {
        }

        public void Start(string selector)
        {
        }
    }
}

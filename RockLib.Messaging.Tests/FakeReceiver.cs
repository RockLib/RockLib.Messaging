using System;

namespace RockLib.Messaging.Tests
{
    public class FakeReceiver : IReceiver
    {
        public string Name { get; set; }
        public string PipeName { get; set; }

        public IMessageHandler MessageHandler { get; set; }

        public event EventHandler Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        public event EventHandler<ErrorEventArgs> Error;

        public void Dispose()
        {
        }
    }
}

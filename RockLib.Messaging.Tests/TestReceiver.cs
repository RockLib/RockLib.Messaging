using System;

namespace RockLib.Messaging.Tests
{
    public class TestReceiver : IReceiver
    {
        public string Name => "TestReceiver";

        public IMessageHandler MessageHandler { get; set; }

        public event EventHandler Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public void Dispose()
        {
        }
    }
}

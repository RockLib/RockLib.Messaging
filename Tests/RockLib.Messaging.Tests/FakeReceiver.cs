using System;

namespace RockLib.Messaging.Tests
{
    public class FakeReceiver : IReceiver
    {
        public string Name { get; set; }
        public string PipeName { get; set; }

        public IMessageHandler MessageHandler { get; set; }

#pragma warning disable CS0067 // Event is never used
        public event EventHandler Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        public event EventHandler<ErrorEventArgs> Error;
#pragma warning restore CS0067 // Event is never used

        public void Dispose()
        {
        }
    }
}

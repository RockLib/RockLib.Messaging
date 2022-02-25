using System;

namespace RockLib.Messaging.Tests
{
    public sealed class FakeReceiver : IReceiver
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
        public string PipeName { get; set; }

        public IMessageHandler MessageHandler { get; set; }

#pragma warning disable CS0067 // Event is never used
        public event EventHandler Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        public event EventHandler<ErrorEventArgs> Error;
#pragma warning restore CS0067 // Event is never used
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Dispose()
        {
        }
    }
}

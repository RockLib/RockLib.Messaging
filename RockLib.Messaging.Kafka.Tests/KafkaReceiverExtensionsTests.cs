using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Kafka.Tests
{
    public class KafkaReceiverExtensionsTests
    {
        [Fact(DisplayName = "Seek extension method undecorates and calls Seek method dynamically")]
        public void SeekHappyPath()
        {
            var fakeReceiver = new FakeKafkaReceiver();
            IReceiver receiver = new ReceiverDecorator(fakeReceiver);

            var expectedTimestamp = DateTime.UtcNow;

            receiver.Seek(expectedTimestamp);

            var timestamp =
                fakeReceiver.SeekInvocations.Should().ContainSingle()
                    .Subject;

            timestamp.Should().Be(expectedTimestamp);

            fakeReceiver.ReplayInvocations.Should().BeEmpty();
        }

        [Fact(DisplayName = "Replay extension method undecorates and calls Replay method dynamically")]
        public async Task ReplayExtensionMethodHappyPath()
        {
            var fakeReceiver = new FakeKafkaReceiver();
            IReceiver receiver = new ReceiverDecorator(fakeReceiver);

            var expectedStart = DateTime.Now;
            var expectedEnd = DateTime.UtcNow;
            Func<IReceiverMessage, Task> expectedCallback = message => Task.CompletedTask;

            await receiver.Replay(expectedStart, expectedEnd, expectedCallback);

            var (start, end, callback) =
                fakeReceiver.ReplayInvocations.Should().ContainSingle()
                    .Subject;

            start.Should().Be(expectedStart);
            end.Should().Be(expectedEnd);
            callback.Should().BeSameAs(expectedCallback);

            fakeReceiver.SeekInvocations.Should().BeEmpty();
        }

        public class ReceiverDecorator : IReceiver
        {
            private readonly IReceiver _receiver;

            public ReceiverDecorator(IReceiver receiver) => _receiver = receiver;

            public string Name => _receiver.Name;

            public IMessageHandler MessageHandler
            {
                get => _receiver.MessageHandler;
                set => _receiver.MessageHandler = value;
            }

            public event EventHandler Connected
            {
                add { _receiver.Connected += value; }
                remove { _receiver.Connected -= value; }
            }

            public event EventHandler<DisconnectedEventArgs> Disconnected
            {
                add { _receiver.Disconnected += value; }
                remove { _receiver.Disconnected -= value; }
            }

            public event EventHandler<ErrorEventArgs> Error
            {
                add { _receiver.Error += value; }
                remove { _receiver.Error -= value; }
            }

            public void Dispose() => _receiver.Dispose();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        public class FakeKafkaReceiver : Receiver
        {
            private readonly List<DateTime> _seekInvocations = new List<DateTime>();
            private readonly List<(DateTime, DateTime?, Func<IReceiverMessage, Task>)> _replayInvocations = new List<(DateTime, DateTime?, Func<IReceiverMessage, Task>)>();

            public FakeKafkaReceiver()
                : base("FakeKafkaReceiver")
            {
            }

            public IReadOnlyList<DateTime> SeekInvocations => _seekInvocations;

            public IReadOnlyList<(DateTime, DateTime?, Func<IReceiverMessage, Task>)> ReplayInvocations => _replayInvocations;

            public void Seek(DateTime timestamp) => _seekInvocations.Add(timestamp);

            public async Task Replay(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback = null) =>
                _replayInvocations.Add((start, end, callback));

            protected override void Start()
            {
            }
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    }
}

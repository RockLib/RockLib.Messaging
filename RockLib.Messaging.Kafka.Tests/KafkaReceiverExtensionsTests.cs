using FluentAssertions;
using RockLib.Messaging.Testing.Kafka;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Kafka.Tests
{
    public class KafkaReceiverExtensionsTests
    {
        [Fact(DisplayName = "Seek extension method undecorates and calls Seek method")]
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

        [Fact(DisplayName = "Replay extension method undecorates and calls Replay method")]
        public async Task ReplayExtensionMethodHappyPath()
        {
            var fakeReceiver = new FakeKafkaReceiver();
            IReceiver receiver = new ReceiverDecorator(fakeReceiver);

            var expectedStart = DateTime.Now;
            var expectedEnd = DateTime.UtcNow;
            Func<IReceiverMessage, Task> expectedCallback = message => Task.CompletedTask;

            await receiver.ReplayAsync(expectedStart, expectedEnd, expectedCallback, true);

            var (start, end, callback, pauseDuringReplay) =
                fakeReceiver.ReplayInvocations.Should().ContainSingle()
                    .Subject;

            start.Should().Be(expectedStart);
            end.Should().Be(expectedEnd);
            callback.Should().BeSameAs(expectedCallback);
            pauseDuringReplay.Should().BeTrue();

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
    }
}

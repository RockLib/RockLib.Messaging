using FluentAssertions;
using RockLib.Messaging.Testing.Kafka;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Kafka.Tests
{
    public class FakeKafkaReceiverTests
    {
        [Fact(DisplayName = null)]
        public void SeekMethodHappyPath()
        {
            var fakeKafkaReceiver = new FakeKafkaReceiver();

            var timestamp1 = new DateTime(2020, 9, 8, 15, 32, 19, DateTimeKind.Local).ToUniversalTime();
            var timestamp2 = new DateTime(2020, 9, 8, 15, 32, 33, DateTimeKind.Local).ToUniversalTime();

            fakeKafkaReceiver.Seek(timestamp1);
            fakeKafkaReceiver.Seek(timestamp2);

            fakeKafkaReceiver.SeekInvocations.Should().HaveCount(2);
            fakeKafkaReceiver.SeekInvocations[0].Should().Be(timestamp1);
            fakeKafkaReceiver.SeekInvocations[1].Should().Be(timestamp2);
        }

        [Fact(DisplayName = null)]
        public void ReplayMethodHappyPath()
        {
            var fakeKafkaReceiver = new FakeKafkaReceiver();

            DateTime start1 = new DateTime(2020, 9, 8, 15, 32, 19, DateTimeKind.Local).ToUniversalTime();
            DateTime? end1 = new DateTime(2020, 9, 8, 15, 35, 3, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback1 = m => Task.CompletedTask;
            DateTime start2 = new DateTime(2020, 9, 8, 15, 32, 33, DateTimeKind.Local).ToUniversalTime();
            DateTime? end2 = null;
            Func<IReceiverMessage, Task> callback2 = null;

            fakeKafkaReceiver.Replay(start1, end1, callback1);
            fakeKafkaReceiver.Replay(start2, end2, callback2);

            var expectedInvocation1 = new ReplayInvocation(start1, end1, callback1);
            var expectedInvocation2 = new ReplayInvocation(start2, end2, callback2);

            fakeKafkaReceiver.ReplayInvocations.Should().HaveCount(2);
            fakeKafkaReceiver.ReplayInvocations[0].Should().Be(expectedInvocation1);
            fakeKafkaReceiver.ReplayInvocations[1].Should().Be(expectedInvocation2);
        }
    }
}

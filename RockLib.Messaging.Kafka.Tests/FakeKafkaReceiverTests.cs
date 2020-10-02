using FluentAssertions;
using RockLib.Messaging.Testing.Kafka;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Kafka.Tests
{
    public class FakeKafkaReceiverTests
    {
        [Fact(DisplayName = "Seek method adds an item to SeekInvocations each time it is called")]
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

        [Fact(DisplayName = "ReplayAsync method adds an item to ReplayInvocations each time it is called")]
        public void ReplayMethodHappyPath()
        {
            var fakeKafkaReceiver = new FakeKafkaReceiver();

            DateTime start1 = new DateTime(2020, 9, 8, 15, 32, 19, DateTimeKind.Local).ToUniversalTime();
            DateTime? end1 = new DateTime(2020, 9, 8, 15, 35, 3, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback1 = m => Task.CompletedTask;
            DateTime start2 = new DateTime(2020, 9, 8, 15, 32, 33, DateTimeKind.Local).ToUniversalTime();
            DateTime? end2 = null;
            Func<IReceiverMessage, Task> callback2 = null;

            fakeKafkaReceiver.ReplayAsync(start1, end1, callback1, true);
            fakeKafkaReceiver.ReplayAsync(start2, end2, callback2, false);

            var expectedInvocation1 = new ReplayInvocation(start1, end1, callback1, true);
            var expectedInvocation2 = new ReplayInvocation(start2, end2, callback2, false);

            fakeKafkaReceiver.ReplayInvocations.Should().HaveCount(2);
            fakeKafkaReceiver.ReplayInvocations[0].Should().Be(expectedInvocation1);
            fakeKafkaReceiver.ReplayInvocations[1].Should().Be(expectedInvocation2);
        }

        [Fact(DisplayName = "Pause method increments PauseInvocations each time it is called")]
        public void PauseMethodHappyPath()
        {
            var fakeKafkaReceiver = new FakeKafkaReceiver();

            fakeKafkaReceiver.Pause();
            fakeKafkaReceiver.Pause();

            fakeKafkaReceiver.PauseInvocations.Should().Be(2);
        }

        [Fact(DisplayName = "Resume method increments ResumeInvocations each time it is called")]
        public void ResumeMethodHappyPath()
        {
            var fakeKafkaReceiver = new FakeKafkaReceiver();

            fakeKafkaReceiver.Resume();
            fakeKafkaReceiver.Resume();
            fakeKafkaReceiver.Resume();

            fakeKafkaReceiver.ResumeInvocations.Should().Be(3);
        }

        [Fact(DisplayName = "Reset method clears each Invocations property")]
        public void ResetMethodHappyPath()
        {
            var fakeKafkaReceiver = new FakeKafkaReceiver();

            DateTime start1 = new DateTime(2020, 9, 8, 15, 32, 19, DateTimeKind.Local).ToUniversalTime();
            DateTime? end1 = new DateTime(2020, 9, 8, 15, 35, 3, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback1 = m => Task.CompletedTask;
            DateTime start2 = new DateTime(2020, 9, 8, 15, 32, 33, DateTimeKind.Local).ToUniversalTime();
            DateTime? end2 = null;
            Func<IReceiverMessage, Task> callback2 = null;

            fakeKafkaReceiver.ReplayAsync(start1, end1, callback1, true);
            fakeKafkaReceiver.ReplayAsync(start2, end2, callback2, false);

            var timestamp1 = new DateTime(2020, 9, 8, 15, 32, 19, DateTimeKind.Local).ToUniversalTime();
            var timestamp2 = new DateTime(2020, 9, 8, 15, 32, 33, DateTimeKind.Local).ToUniversalTime();

            fakeKafkaReceiver.Seek(timestamp1);
            fakeKafkaReceiver.Seek(timestamp2);

            fakeKafkaReceiver.Pause();
            fakeKafkaReceiver.Pause();

            fakeKafkaReceiver.Resume();
            fakeKafkaReceiver.Resume();
            fakeKafkaReceiver.Resume();

            fakeKafkaReceiver.Reset();

            fakeKafkaReceiver.ReplayInvocations.Should().BeEmpty();
            fakeKafkaReceiver.SeekInvocations.Should().BeEmpty();
            fakeKafkaReceiver.PauseInvocations.Should().Be(0);
            fakeKafkaReceiver.ResumeInvocations.Should().Be(0);
        }
    }
}

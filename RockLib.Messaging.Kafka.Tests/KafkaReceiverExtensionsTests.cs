using FluentAssertions;
using Moq;
using RockLib.Dynamic;
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

            var expectedTimestamp = new DateTime(2020, 9, 29, 17, 19, 28, DateTimeKind.Local);

            receiver.Seek(expectedTimestamp);

            var timestamp =
                fakeReceiver.SeekInvocations.Should().ContainSingle()
                    .Subject;

            timestamp.Should().Be(expectedTimestamp);

            fakeReceiver.ReplayInvocations.Should().BeEmpty();
        }

        [Fact(DisplayName = "Seek extension method throws if receiver parameter is null")]
        public void SeekSadPath1()
        {
            IReceiver receiver = null;

            var timestamp = new DateTime(2020, 9, 29, 17, 19, 28, DateTimeKind.Local);

            Action act = () => receiver.Seek(timestamp);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact(DisplayName = "Seek extension method throws if receiver is not a kafka receiver")]
        public void SeekSadPath2()
        {
            var receiver = new Mock<IReceiver>().Object;

            var timestamp = new DateTime(2020, 9, 29, 17, 19, 28, DateTimeKind.Local);

            Action act = () => receiver.Seek(timestamp);

            act.Should().ThrowExactly<ArgumentException>().WithMessage("Must be a kafka receiver or a decorator for a kafka receiver.*receiver*");
        }

        [Fact(DisplayName = "Pause extension method undecorates and calls Pause method")]
        public void PauseHappyPath()
        {
            var fakeReceiver = new FakeKafkaReceiver();
            IReceiver receiver = new ReceiverDecorator(fakeReceiver);

            receiver.Pause();
            receiver.Pause();

            fakeReceiver.PauseInvocations.Should().Be(2);
        }

        [Fact(DisplayName = "Pause extension method throws if receiver parameter is null")]
        public void PauseSadPath1()
        {
            IReceiver receiver = null;

            Action act = () => receiver.Pause();

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact(DisplayName = "Pause extension method throws if receiver is not a kafka receiver")]
        public void PauseSadPath2()
        {
            var receiver = new Mock<IReceiver>().Object;

            Action act = () => receiver.Pause();

            act.Should().ThrowExactly<ArgumentException>().WithMessage("Must be a kafka receiver or a decorator for a kafka receiver.*receiver*");
        }

        [Fact(DisplayName = "Resume extension method undecorates and calls Resume method")]
        public void ResumeHappyPath()
        {
            var fakeReceiver = new FakeKafkaReceiver();
            IReceiver receiver = new ReceiverDecorator(fakeReceiver);

            receiver.Resume();
            receiver.Resume();

            fakeReceiver.ResumeInvocations.Should().Be(2);
        }

        [Fact(DisplayName = "Resume extension method throws if receiver parameter is null")]
        public void ResumeSadPath1()
        {
            IReceiver receiver = null;

            Action act = () => receiver.Resume();

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact(DisplayName = "Resume extension method throws if receiver is not a kafka receiver")]
        public void ResumeSadPath2()
        {
            var receiver = new Mock<IReceiver>().Object;

            Action act = () => receiver.Resume();

            act.Should().ThrowExactly<ArgumentException>().WithMessage("Must be a kafka receiver or a decorator for a kafka receiver.*receiver*");
        }

        [Fact(DisplayName = "Replay extension method undecorates and calls Replay method")]
        public async Task ReplayExtensionMethodHappyPath()
        {
            var fakeReceiver = new FakeKafkaReceiver();
            IReceiver receiver = new ReceiverDecorator(fakeReceiver);

            var expectedStart = new DateTime(2020, 9, 29, 17, 22, 26, DateTimeKind.Local);
            var expectedEnd = new DateTime(2020, 9, 29, 17, 22, 32, DateTimeKind.Local);
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

        [Fact(DisplayName = "Replay extension method throws if receiver parameter is null")]
        public async Task ReplayExtensionMethodSadPath1()
        {
            IReceiver receiver = null;

            var expectedStart = new DateTime(2020, 9, 29, 17, 22, 26, DateTimeKind.Local);
            var expectedEnd = new DateTime(2020, 9, 29, 17, 22, 32, DateTimeKind.Local);
            Func<IReceiverMessage, Task> expectedCallback = message => Task.CompletedTask;

            Func<Task> act = () => receiver.ReplayAsync(expectedStart, expectedEnd, expectedCallback, true);

            await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact(DisplayName = "Replay extension method throws if receiver is not a kafka receiver")]
        public async Task ReplayExtensionMethodSadPath2()
        {
            var receiver = new Mock<IReceiver>().Object;

            var expectedStart = new DateTime(2020, 9, 29, 17, 22, 26, DateTimeKind.Local);
            var expectedEnd = new DateTime(2020, 9, 29, 17, 22, 32, DateTimeKind.Local);
            Func<IReceiverMessage, Task> expectedCallback = message => Task.CompletedTask;

            Func<Task> act = () => receiver.ReplayAsync(expectedStart, expectedEnd, expectedCallback, true);

            await act.Should().ThrowExactlyAsync<ArgumentException>().WithMessage("Must be a kafka receiver or a decorator for a kafka receiver.*receiver*");
        }

        [Fact(DisplayName = "Start method 1 sets StartTimestamp and starts the receiver")]
        public void StartMethod1HappyPath()
        {
            var kafkaReceiver = new FakeKafkaReceiver();
            var receiver = new ReceiverDecorator(kafkaReceiver);
            
            var messageHandler = new Mock<IMessageHandler>().Object;
            var startTimestamp = new DateTime(2020, 9, 29, 15, 15, 58, DateTimeKind.Local).ToUniversalTime();

            receiver.Start(messageHandler, startTimestamp);

            kafkaReceiver.StartTimestamp.Should().Be(startTimestamp);
            receiver.MessageHandler.Should().Be(messageHandler);
        }

        [Fact(DisplayName = "Start method 1 throws if receiver parameter is null")]
        public void StartMethod1SadPath1()
        {
            IReceiver receiver = null;

            var messageHandler = new Mock<IMessageHandler>().Object;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 1, 52, DateTimeKind.Local);

            Action act = () => receiver.Start(messageHandler, startTimestamp);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact(DisplayName = "Start method 1 throws if messageHandler parameter is null")]
        public void StartMethod1SadPath2()
        {
            var kafkaReceiver = new FakeKafkaReceiver();
            var receiver = new ReceiverDecorator(kafkaReceiver);

            IMessageHandler messageHandler = null;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 1, 52, DateTimeKind.Local);

            Action act = () => receiver.Start(messageHandler, startTimestamp);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*messageHandler*");
        }

        [Fact(DisplayName = "Start method 1 throws if receiver is not a kafka receiver")]
        public void StartMethod1SadPath3()
        {
            var receiver = new Mock<IReceiver>().Object;

            var messageHandler = new Mock<IMessageHandler>().Object;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 4, 47, DateTimeKind.Local);

            Action act = () => receiver.Start(messageHandler, startTimestamp);

            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("Must be a kafka receiver or a decorator for a kafka receiver.*receiver*");
        }

        [Fact(DisplayName = "Start method 2 sets StartTimestamp and starts the receiver")]
        public void StartMethod2HappyPath()
        {
            var kafkaReceiver = new FakeKafkaReceiver();
            var receiver = new ReceiverDecorator(kafkaReceiver);

            OnMessageReceivedAsyncDelegate onMessageReceivedAsync = (r, m) => Task.CompletedTask;
            var startTimestamp = new DateTime(2020, 9, 29, 15, 15, 58, DateTimeKind.Local).ToUniversalTime();

            receiver.Start(onMessageReceivedAsync, startTimestamp);

            kafkaReceiver.StartTimestamp.Should().Be(startTimestamp);

            OnMessageReceivedAsyncDelegate actualOnMessageReceivedAsync =
                receiver.MessageHandler.Unlock()._onMessageReceivedAsync;
            actualOnMessageReceivedAsync.Should().BeSameAs(onMessageReceivedAsync);
        }

        [Fact(DisplayName = "Start method 2 throws if receiver parameter is null")]
        public void StartMethod2SadPath1()
        {
            IReceiver receiver = null;

            OnMessageReceivedAsyncDelegate onMessageReceivedAsync = (r, m) => Task.CompletedTask;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 1, 52, DateTimeKind.Local);

            Action act = () => receiver.Start(onMessageReceivedAsync, startTimestamp);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact(DisplayName = "Start method 2 throws if onMessageReceivedAsync parameter is null")]
        public void StartMethod2SadPath2()
        {
            var kafkaReceiver = new FakeKafkaReceiver();
            var receiver = new ReceiverDecorator(kafkaReceiver);

            OnMessageReceivedAsyncDelegate onMessageReceivedAsync = null;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 1, 52, DateTimeKind.Local);

            Action act = () => receiver.Start(onMessageReceivedAsync, startTimestamp);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*onMessageReceivedAsync*");
        }

        [Fact(DisplayName = "Start method 2 throws if receiver is not a kafka receiver")]
        public void StartMethod2SadPath3()
        {
            var receiver = new Mock<IReceiver>().Object;

            OnMessageReceivedAsyncDelegate onMessageReceivedAsync = (r, m) => Task.CompletedTask;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 4, 47, DateTimeKind.Local);

            Action act = () => receiver.Start(onMessageReceivedAsync, startTimestamp);

            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("Must be a kafka receiver or a decorator for a kafka receiver.*receiver*");
        }

        [Fact(DisplayName = "Start method 3 sets StartTimestamp and starts the receiver")]
        public void StartMethod3HappyPath()
        {
            var kafkaReceiver = new FakeKafkaReceiver();
            var receiver = new ReceiverDecorator(kafkaReceiver);

            Func<IReceiverMessage, Task> onMessageReceivedAsync = m => Task.CompletedTask;
            var startTimestamp = new DateTime(2020, 9, 29, 15, 15, 58, DateTimeKind.Local).ToUniversalTime();

            receiver.Start(onMessageReceivedAsync, startTimestamp);

            kafkaReceiver.StartTimestamp.Should().Be(startTimestamp);

            OnMessageReceivedAsyncDelegate outerDelegate =
                receiver.MessageHandler.Unlock()._onMessageReceivedAsync;
            Func<IReceiverMessage, Task> actualOnMessageReceivedAsync =
                outerDelegate.Target.Unlock().onMessageReceivedAsync;
            actualOnMessageReceivedAsync.Should().BeSameAs(onMessageReceivedAsync);
        }

        [Fact(DisplayName = "Start method 3 throws if receiver parameter is null")]
        public void StartMethod3SadPath1()
        {
            IReceiver receiver = null;

            Func<IReceiverMessage, Task> onMessageReceivedAsync = m => Task.CompletedTask;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 1, 52, DateTimeKind.Local);

            Action act = () => receiver.Start(onMessageReceivedAsync, startTimestamp);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiver*");
        }

        [Fact(DisplayName = "Start method 3 throws if onMessageReceivedAsync parameter is null")]
        public void StartMethod3SadPath2()
        {
            var kafkaReceiver = new FakeKafkaReceiver();
            var receiver = new ReceiverDecorator(kafkaReceiver);

            Func<IReceiverMessage, Task> onMessageReceivedAsync = null;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 1, 52, DateTimeKind.Local);

            Action act = () => receiver.Start(onMessageReceivedAsync, startTimestamp);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*onMessageReceivedAsync*");
        }

        [Fact(DisplayName = "Start method 3 throws if receiver is not a kafka receiver")]
        public void StartMethod3SadPath3()
        {
            var receiver = new Mock<IReceiver>().Object;

            Func<IReceiverMessage, Task> onMessageReceivedAsync = m => Task.CompletedTask;
            var startTimestamp = new DateTime(2020, 9, 29, 17, 4, 47, DateTimeKind.Local);

            Action act = () => receiver.Start(onMessageReceivedAsync, startTimestamp);

            act.Should().ThrowExactly<ArgumentException>()
                .WithMessage("Must be a kafka receiver or a decorator for a kafka receiver.*receiver*");
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

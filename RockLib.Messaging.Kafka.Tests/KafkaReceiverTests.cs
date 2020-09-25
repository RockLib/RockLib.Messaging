using FluentAssertions;
using Xunit;
using System;
using Confluent.Kafka;
using Moq;
using RockLib.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using RockLib.Messaging.Testing;

namespace RockLib.Messaging.Kafka.Tests
{
    public class KafkaReceiverTests
    {
        [Fact(DisplayName = "KafkaReceiver constructor 1 sets appropriate properties")]
        public void KafkaReceiverConstructor1HappyPath()
        {
            var name = "name";
            var topic = "topic";
            var groupId = "groupId";
            var servers = "bootstrapServers";
            var sender = new KafkaReceiver(name, topic, groupId, servers);

            sender.Name.Should().Be(name);
            sender.Topic.Should().Be(topic);
            sender.Consumer.Should().NotBeNull();
        }

        [Fact(DisplayName = "KafkaReceiver constructor 1 throws on null topic")]
        public void KafkaReceiverConstructor1SadPath1()
        {
            Action action = () => new KafkaReceiver("name", null, "groupId", "servers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver constructor 1 throws on null groupId")]
        public void KafkaReceiverConstructor1SadPath2()
        {
            Action action = () => new KafkaReceiver("name", "topic", null, "servers");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver constructor 1 throws on null bootstrapServers")]
        public void KafkaReceiverConstructor1SadPath3()
        {
            Action action = () => new KafkaReceiver("name", "topic", "groupId", null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "KafkaReceiver Start starts the receiver and tracking threads")]
        public void KafkaReceiverStartHappyPath()
        {
            var receiver = new KafkaReceiver("name", "one_topic", "groupId", "servers");

            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            consumerMock.Setup(c => c.Subscribe(It.IsAny<string>()));

            var unlockedReceiver = receiver.Unlock();
            unlockedReceiver._consumer = new Lazy<IConsumer<string, byte[]>>(() => consumerMock.Object);
            unlockedReceiver.Start();

            consumerMock.Verify(cm => cm.Subscribe("one_topic"), Times.Once);

            unlockedReceiver.Dispose();
        }

        [Fact(DisplayName = "KafkaReceiver Dispose stops the receiver and tracking threads")]
        public void KafkaReceiverDisposeHappyPath()
        {
            var receiver = new KafkaReceiver("name", "one_topic", "groupId", "servers");

            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            consumerMock.Setup(cm => cm.Close());
            consumerMock.Setup(cm => cm.Dispose());

            var unlockedReceiver = receiver.Unlock();
            unlockedReceiver._consumer = new Lazy<IConsumer<string, byte[]>>(() => consumerMock.Object);
            unlockedReceiver.Start();

            unlockedReceiver.Dispose();

            consumerMock.Verify(cm => cm.Close(), Times.Once);
            consumerMock.Verify(cm => cm.Dispose(), Times.Once);
        }

        [Fact(DisplayName = "KafkaReceiver receives message from consumer")]
        public void KafkaReceiverHappyPath()
        {
            var message = new Message<string, byte[]>() { Value = Encoding.UTF8.GetBytes("This is the expected message!") };
            var result = new ConsumeResult<string, byte[]>() { Message = message };

            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            consumerMock.Setup(c => c.Subscribe(It.IsAny<string>()));
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>())).Returns(result);

            var waitHandle = new AutoResetEvent(false);

            string receivedMessage = null;

            using (var receiver = new KafkaReceiver("NAME", "TOPIC", "GROUPID", "SERVER"))
            {
                var unlockedReceiver = receiver.Unlock();
                unlockedReceiver._consumer = new Lazy<IConsumer<string, byte[]>>(() => consumerMock.Object);

                receiver.Start(m =>
                {
                    receivedMessage = m.StringPayload;
                    waitHandle.Set();
                    return Task.CompletedTask;
                });

                waitHandle.WaitOne();
            }

            consumerMock.Verify(m => m.Consume(It.IsAny<CancellationToken>()));

            receivedMessage.Should().Be("This is the expected message!");
        }

        [Fact(DisplayName = "Seek method calls Consumer")]
        public void SeekMethodHappyPath()
        {
            var assignments = new List<TopicPartition>
            {
                new TopicPartition("one_topic", new Partition(0)),
                new TopicPartition("one_topic", new Partition(1))
            };
            var offsetsForTimes = new List<TopicPartitionOffset>
            {
                new TopicPartitionOffset(assignments[0], new Offset(1)),
                new TopicPartitionOffset(assignments[1], new Offset(2))
            };
            TopicPartitionTimestamp[] actualTimestamps = null;

            Action<IEnumerable<TopicPartitionTimestamp>, TimeSpan> callback = (timestamps, timeout) =>
                actualTimestamps = timestamps.ToArray();

            var mockConsumer = new Mock<IConsumer<string, byte[]>>();
            mockConsumer.Setup(m => m.Assignment).Returns(assignments);
            mockConsumer.Setup(m => m.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(offsetsForTimes)
                .Callback(callback);

            var lazyConsumer = new Lazy<IConsumer<string, byte[]>>(() => mockConsumer.Object);

            var receiver = new KafkaReceiver("name", "one_topic", "groupId", "servers");

            receiver.Unlock()._consumer = lazyConsumer;

            var timestamp = new DateTime(2020, 9, 3, 17, 52, 37, DateTimeKind.Local).ToUniversalTime();

            receiver.Seek(timestamp);

            mockConsumer.Verify(m => m.Seek(offsetsForTimes[0]), Times.Once());
            mockConsumer.Verify(m => m.Seek(offsetsForTimes[1]), Times.Once());

            var expectedTimestampsToSearch = assignments.Select(tp => new TopicPartitionTimestamp(tp, new Timestamp(timestamp))).ToArray();

            mockConsumer.Verify(m => m.OffsetsForTimes(expectedTimestampsToSearch, It.IsAny<TimeSpan>()), Times.Once());

            actualTimestamps.Should().HaveCount(2);
            actualTimestamps.Select(t => t.TopicPartition).Should().BeEquivalentTo(assignments);
            actualTimestamps.Select(t => t.Timestamp.UtcDateTime).Should().AllBeEquivalentTo(timestamp);
        }

        [Fact(DisplayName = "Seek method throws when called before receiver is started")]
        public void SeekMethodSadPath()
        {
            var receiver = new KafkaReceiver("name", "one_topic", "groupId", "servers");

            Action act = () => receiver.Seek(DateTime.Now);

            act.Should().ThrowExactly<InvalidOperationException>()
                .WithMessage("Seek cannot be called before the receiver has been started.");
        }

        [Fact(DisplayName = "Replay method passes parameters and properties to ReplayEngine")]
        public async Task ReplayMethodHappyPath1()
        {
            var mockReplayEngine = new Mock<IReplayEngine>();
            mockReplayEngine.Setup(m => m.Replay(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<Func<IReceiverMessage, Task>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<AutoOffsetReset>()))
                .Returns(Task.CompletedTask);

            var receiver =
                new KafkaReceiver("name", "one_topic", "groupId", "servers", true, AutoOffsetReset.Earliest, mockReplayEngine.Object);

            var start = new DateTime(2020, 9, 3, 20, 22, 58, DateTimeKind.Local).ToUniversalTime();
            var end = new DateTime(2020, 9, 3, 20, 23, 19, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback = message => Task.CompletedTask;

            await receiver.Replay(start, end, callback);

            mockReplayEngine.Verify(m =>
                m.Replay(start, end, callback, "one_topic", "servers", true, AutoOffsetReset.Earliest),
                Times.Once());
        }

        [Fact(DisplayName = "Replay method passes MessageHandler property when callback parameter is null")]
        public async Task ReplayMethodHappyPath2()
        {
            Func<IReceiverMessage, Task> capturedCallback = null;

            Action<DateTime, DateTime?, Func<IReceiverMessage, Task>, string, string, bool, AutoOffsetReset> mockReplayEngineCallback =
                (start, end, callback, topic, bootstrapServers, enableAutoOffsetStore, autoOffsetReset) =>
                {
                    capturedCallback = callback;
                };

            var mockReplayEngine = new Mock<IReplayEngine>();
            mockReplayEngine.Setup(m => m.Replay(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<Func<IReceiverMessage, Task>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<AutoOffsetReset>()))
                .Returns(Task.CompletedTask)
                .Callback(mockReplayEngineCallback);

            var mockMessageHandler = new Mock<IMessageHandler>();
            mockMessageHandler.Setup(m => m.OnMessageReceivedAsync(It.IsAny<IReceiver>(), It.IsAny<IReceiverMessage>()))
                .Returns(Task.CompletedTask);

            var receiver =
                new KafkaReceiver("name", "one_topic", "groupId", "servers", true, AutoOffsetReset.Earliest, mockReplayEngine.Object);

            receiver.MessageHandler = mockMessageHandler.Object;

            var expectedStart = new DateTime(2020, 9, 3, 20, 22, 58, DateTimeKind.Local).ToUniversalTime();
            var expectedEnd = new DateTime(2020, 9, 3, 20, 23, 19, DateTimeKind.Local).ToUniversalTime();

            await receiver.Replay(expectedStart, expectedEnd, null);

            mockReplayEngine.Verify(m =>
                m.Replay(expectedStart, expectedEnd, It.IsAny<Func<IReceiverMessage, Task>>(), "one_topic", "servers", true, AutoOffsetReset.Earliest),
                Times.Once());

            capturedCallback.Should().NotBeNull();

            var message = new FakeReceiverMessage("Hello, world!");

            await capturedCallback(message);

            mockMessageHandler.Verify(m => m.OnMessageReceivedAsync(receiver, message), Times.Once());
        }

        [Fact(DisplayName = "Replay method pauses and resumes the consumer if pauseDuringReplay is true")]
        public async Task ReplayMethodHappyPath3()
        {
            var sequence = new MockSequence();
            var mockConsumer = new Mock<IConsumer<string, byte[]>>(MockBehavior.Strict);
            var mockReplayEngine = new Mock<IReplayEngine>(MockBehavior.Strict);

            var topicPartitions = new List<TopicPartition> { new TopicPartition("MyTopic", new Partition(0)) };
            mockConsumer.InSequence(sequence).Setup(m => m.Assignment).Returns(topicPartitions);
            mockConsumer.InSequence(sequence).Setup(m => m.Pause(topicPartitions));

            mockReplayEngine.InSequence(sequence).Setup(m => m.Replay(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<Func<IReceiverMessage, Task>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<AutoOffsetReset>()))
                .Returns(Task.CompletedTask);
            
            mockConsumer.InSequence(sequence).Setup(m => m.Resume(topicPartitions));

            var receiver =
                new KafkaReceiver("name", "one_topic", "groupId", "servers", true, AutoOffsetReset.Earliest, mockReplayEngine.Object);

            receiver.Unlock()._consumer = new Lazy<IConsumer<string, byte[]>>(() => mockConsumer.Object);

            var start = new DateTime(2020, 9, 3, 20, 22, 58, DateTimeKind.Local).ToUniversalTime();
            var end = new DateTime(2020, 9, 3, 20, 23, 19, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback = message => Task.CompletedTask;

            await receiver.Replay(start, end, callback, pauseDuringReplay: true);

            mockReplayEngine.Verify(m =>
                m.Replay(start, end, callback, "one_topic", "servers", true, AutoOffsetReset.Earliest),
                Times.Once());

            mockConsumer.Verify(m => m.Pause(topicPartitions), Times.Once());
            mockConsumer.Verify(m => m.Resume(topicPartitions), Times.Once());
        }

        [Fact(DisplayName = "Replay method throws when callback parameter and MessageHandler property are both null")]
        public async Task ReplayMethodSadPath()
        {
            var mockReplayEngine = new Mock<IReplayEngine>();
            mockReplayEngine.Setup(m => m.Replay(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<Func<IReceiverMessage, Task>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<AutoOffsetReset>()))
                .Returns(Task.CompletedTask);

            var receiver =
                new KafkaReceiver("name", "one_topic", "groupId", "servers", true, AutoOffsetReset.Earliest, mockReplayEngine.Object);

            var start = new DateTime(2020, 9, 3, 20, 22, 58, DateTimeKind.Local).ToUniversalTime();
            var end = new DateTime(2020, 9, 3, 20, 23, 19, DateTimeKind.Local).ToUniversalTime();

            Func<Task> act = async () => await receiver.Replay(start, end, null);

            (await act.Should().ThrowExactlyAsync<InvalidOperationException>())
                .WithMessage("Replay cannot be called with a null 'callback' parameter before the receiver has been started.");

            mockReplayEngine.Verify(m =>
                m.Replay(It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<Func<IReceiverMessage, Task>>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<AutoOffsetReset>()),
                Times.Never());
        }
    }
}

using Confluent.Kafka;
using FluentAssertions;
using Moq;
using Moq.Protected;
using RockLib.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Kafka.Tests
{
    public class DefaultReplayEngineTests
    {
        [Fact(DisplayName = "Replay method does the right thing")]
        public async Task ReplayMethodHappyPath1()
        {
            var offsets = new List<TopicPartitionOffset> { new TopicPartitionOffset("MyTopic", new Partition(0), Offset.End) };

            var mockConsumer = new Mock<IConsumer<string, byte[]>>();
            mockConsumer.Setup(m => m.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(offsets);

            var timestamp = new DateTime(2020, 9, 4, 11, 34, 59, DateTimeKind.Local).ToUniversalTime();
            var timestamps = new[] { new TopicPartitionTimestamp("MyTopic", new Partition(0), new Timestamp(timestamp)) };

            var mockEngine = new Mock<DefaultReplayEngine>();
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetConsumer(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<AutoOffsetReset>()))
                .Returns(mockConsumer.Object);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetStartTimestamps(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(timestamps);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.Replay(It.IsAny<IConsumer<string, byte[]>>(), It.IsAny<List<TopicPartitionOffset>>(), It.IsAny<DateTime>(), It.IsAny<Func<IReceiverMessage, Task>>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var engine = mockEngine.Object;

            var start = new DateTime(2020, 9, 4, 11, 34, 54, DateTimeKind.Local).ToUniversalTime();
            var end = new DateTime(2020, 9, 4, 11, 35, 6, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback = m => Task.CompletedTask;
            var topic = "MyTopic";
            var bootstrapServers = "MyServers";
            var enableAutoOffsetStore = true;
            var autoOffsetReset = AutoOffsetReset.Earliest;

            await engine.Replay(start, end, callback, topic, bootstrapServers, enableAutoOffsetStore, autoOffsetReset);

            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetConsumer(bootstrapServers, enableAutoOffsetStore, autoOffsetReset), Times.Once());
            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetStartTimestamps(topic, bootstrapServers, start), Times.Once());
            mockConsumer
                .Verify(m => m.OffsetsForTimes(timestamps, It.IsAny<TimeSpan>()), Times.Once());
            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.Replay(mockConsumer.Object, offsets, end, callback, enableAutoOffsetStore), Times.Once());
        }

        [Fact(DisplayName = "Replay method uses UtcNow when end parameter is null")]
        public async Task ReplayMethodHappyPath2()
        {
            var offsets = new List<TopicPartitionOffset> { new TopicPartitionOffset("MyTopic", new Partition(0), Offset.End) };

            var mockConsumer = new Mock<IConsumer<string, byte[]>>();
            mockConsumer.Setup(m => m.OffsetsForTimes(It.IsAny<IEnumerable<TopicPartitionTimestamp>>(), It.IsAny<TimeSpan>()))
                .Returns(offsets);

            var timestamp = new DateTime(2020, 9, 4, 11, 34, 59, DateTimeKind.Local).ToUniversalTime();
            var timestamps = new[] { new TopicPartitionTimestamp("MyTopic", new Partition(0), new Timestamp(timestamp)) };

            var mockEngine = new Mock<DefaultReplayEngine>();
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetUtcNow())
                .Returns(timestamp);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetConsumer(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<AutoOffsetReset>()))
                .Returns(mockConsumer.Object);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetStartTimestamps(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(timestamps);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.Replay(It.IsAny<IConsumer<string, byte[]>>(), It.IsAny<List<TopicPartitionOffset>>(), It.IsAny<DateTime>(), It.IsAny<Func<IReceiverMessage, Task>>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            var engine = mockEngine.Object;

            var start = new DateTime(2020, 9, 4, 11, 34, 54, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback = m => Task.CompletedTask;
            var topic = "MyTopic";
            var bootstrapServers = "MyServers";
            var enableAutoOffsetStore = true;
            var autoOffsetReset = AutoOffsetReset.Earliest;

            await engine.Replay(start, null, callback, topic, bootstrapServers, enableAutoOffsetStore, autoOffsetReset);

            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetConsumer(bootstrapServers, enableAutoOffsetStore, autoOffsetReset), Times.Once());
            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetStartTimestamps(topic, bootstrapServers, start), Times.Once());
            mockConsumer
                .Verify(m => m.OffsetsForTimes(timestamps, It.IsAny<TimeSpan>()), Times.Once());
            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.Replay(mockConsumer.Object, offsets, timestamp, callback, enableAutoOffsetStore), Times.Once());
        }

        [Fact(DisplayName = "Replay method throws when end is earlier than start")]
        public async Task ReplayMethodSadPath1()
        {
            var engine = new DefaultReplayEngine();

            var start = new DateTime(2020, 9, 4, 11, 29, 1, DateTimeKind.Local).ToUniversalTime();
            var end = new DateTime(2020, 9, 4, 11, 28, 31, DateTimeKind.Local).ToUniversalTime();

            Func<Task> act = () => engine.Replay(start, end, m => Task.CompletedTask, "MyTopic", "MyServers", false, AutoOffsetReset.Latest);

            (await act.Should().ThrowExactlyAsync<ArgumentException>())
                .WithMessage("Cannot be earlier than 'start' parameter.*end*");
        }

        [Fact(DisplayName = "Replay method throws when start is later than UtcNow and end is null")]
        public async Task ReplayMethodSadPath2()
        {
            var engine = new DefaultReplayEngine();

            var start = DateTime.UtcNow + TimeSpan.FromDays(1);

            Func<Task> act = () => engine.Replay(start, null, m => Task.CompletedTask, "MyTopic", "MyServers", false, AutoOffsetReset.Latest);

            (await act.Should().ThrowExactlyAsync<ArgumentException>())
                .WithMessage("Cannot be later than DateTime.UtcNow when 'end' parameter is null.*start*");
        }

        [Fact(DisplayName = "Replay method throws when callback is null")]
        public async Task ReplayMethodSadPath3()
        {
            var engine = new DefaultReplayEngine();

            var start = new DateTime(2020, 9, 4, 11, 34, 54, DateTimeKind.Local).ToUniversalTime();
            var end = new DateTime(2020, 9, 4, 11, 35, 6, DateTimeKind.Local).ToUniversalTime();

            Func<Task> act = () => engine.Replay(start, end, null, "MyTopic", "MyServers", false, AutoOffsetReset.Latest);

            (await act.Should().ThrowExactlyAsync<ArgumentNullException>())
                .WithMessage("*callback*");
        }

        [Theory(DisplayName = "Replay method throws when topic is null or empty")]
        [InlineData(null)]
        [InlineData("")]
        public async Task ReplayMethodSadPath4(string topic)
        {
            var engine = new DefaultReplayEngine();

            var start = new DateTime(2020, 9, 4, 11, 34, 54, DateTimeKind.Local).ToUniversalTime();
            var end = new DateTime(2020, 9, 4, 11, 35, 6, DateTimeKind.Local).ToUniversalTime();

            Func<Task> act = () => engine.Replay(start, end, m => Task.CompletedTask, topic, "MyServers", false, AutoOffsetReset.Latest);

            (await act.Should().ThrowExactlyAsync<ArgumentNullException>())
                .WithMessage("*topic*");
        }

        [Theory(DisplayName = "Replay method throws when bootstrapServers is null or empty")]
        [InlineData(null)]
        [InlineData("")]
        public async Task ReplayMethodSadPath5(string bootstrapServers)
        {
            var engine = new DefaultReplayEngine();

            var start = new DateTime(2020, 9, 4, 11, 34, 54, DateTimeKind.Local).ToUniversalTime();
            var end = new DateTime(2020, 9, 4, 11, 35, 6, DateTimeKind.Local).ToUniversalTime();

            Func<Task> act = () => engine.Replay(start, end, m => Task.CompletedTask, "MyTopic", bootstrapServers, false, AutoOffsetReset.Latest);

            (await act.Should().ThrowExactlyAsync<ArgumentNullException>())
                .WithMessage("*bootstrapServers*");
        }

        [Fact(DisplayName = "GetUtcNow protected method returns DateTime.UtcNow")]
        public void GetUtcNowProtectedMethodHappyPath()
        {
            var engine = new DefaultReplayEngine().Unlock();

            var before = DateTime.UtcNow;

            DateTime utcNow = engine.GetUtcNow();
            
            var after = DateTime.UtcNow;

            utcNow.Should().BeIn(DateTimeKind.Utc);
            utcNow.Should().BeOnOrAfter(before);
            utcNow.Should().BeOnOrBefore(after);
        }

        [Fact(DisplayName = "GetStartTimestamps protected method returns a timestamp for each topic and partition")]
        public void GetStartTimestampsProtectedMethodHappyPath1()
        {
            var topic1Partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, Array.Empty<int>(), Array.Empty<int>(), null),
                new PartitionMetadata(1, 0, Array.Empty<int>(), Array.Empty<int>(), null)
            };
            var topic2Partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(2, 0, Array.Empty<int>(), Array.Empty<int>(), null),
                new PartitionMetadata(3, 0, Array.Empty<int>(), Array.Empty<int>(), null)
            };
            var topics = new List<TopicMetadata>
            {
                new TopicMetadata("Topic1", topic1Partitions, null),
                new TopicMetadata("Topic2", topic2Partitions, null)
            };
            var metadata = new Metadata(new List<BrokerMetadata>(), topics, 0, "BrokerName");

            var mockAdminClient = new Mock<IAdminClient>();
            mockAdminClient.Setup(m => m.GetMetadata(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(metadata);

            var mockEngine = new Mock<DefaultReplayEngine>();
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetAdminClient(It.IsAny<string>()))
                .Returns(mockAdminClient.Object);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetStartTimestamps(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .CallBase();

            var engine = mockEngine.Object.Unlock();

            var topic = "MyTopic";
            var bootstrapServers = "MyServers";
            var start = new DateTime(2020, 9, 4, 14, 40, 16, DateTimeKind.Local).ToUniversalTime();

            IEnumerable<TopicPartitionTimestamp> t = engine.GetStartTimestamps(topic, bootstrapServers, start);
            var startTimestamps = t.ToArray();

            startTimestamps.Should().Contain(tpt => tpt.Topic == "Topic1" && tpt.Partition.Value == 0 && tpt.Timestamp.UtcDateTime == start);
            startTimestamps.Should().Contain(tpt => tpt.Topic == "Topic1" && tpt.Partition.Value == 1 && tpt.Timestamp.UtcDateTime == start);
            startTimestamps.Should().Contain(tpt => tpt.Topic == "Topic2" && tpt.Partition.Value == 2 && tpt.Timestamp.UtcDateTime == start);
            startTimestamps.Should().Contain(tpt => tpt.Topic == "Topic2" && tpt.Partition.Value == 3 && tpt.Timestamp.UtcDateTime == start);

            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetAdminClient(bootstrapServers), Times.Once());

            mockAdminClient.Verify(m => m.GetMetadata(topic, It.IsAny<TimeSpan>()), Times.Once());
        }

        [Fact(DisplayName = "GetStartTimestamps protected method handles null Metadata")]
        public void GetStartTimestampsProtectedMethodHappyPath2()
        {
            var mockAdminClient = new Mock<IAdminClient>();
            mockAdminClient.Setup(m => m.GetMetadata(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns((Metadata)null);

            var mockEngine = new Mock<DefaultReplayEngine>();
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetAdminClient(It.IsAny<string>()))
                .Returns(mockAdminClient.Object);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetStartTimestamps(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .CallBase();

            var engine = mockEngine.Object.Unlock();

            var topic = "MyTopic";
            var bootstrapServers = "MyServers";
            var start = new DateTime(2020, 9, 4, 14, 40, 16, DateTimeKind.Local).ToUniversalTime();

            IEnumerable<TopicPartitionTimestamp> t = engine.GetStartTimestamps(topic, bootstrapServers, start);
            var startTimestamps = t.ToArray();

            startTimestamps.Should().BeEmpty();

            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetAdminClient(bootstrapServers), Times.Once());

            mockAdminClient.Verify(m => m.GetMetadata(topic, It.IsAny<TimeSpan>()), Times.Once());
        }

        [Fact(DisplayName = "GetStartTimestamps protected method handles null Metadata.Topics")]
        public void GetStartTimestampsProtectedMethodHappyPath3()
        {
            var metadata = new Metadata(new List<BrokerMetadata>(), null, 0, "BrokerName");

            var mockAdminClient = new Mock<IAdminClient>();
            mockAdminClient.Setup(m => m.GetMetadata(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(metadata);

            var mockEngine = new Mock<DefaultReplayEngine>();
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetAdminClient(It.IsAny<string>()))
                .Returns(mockAdminClient.Object);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetStartTimestamps(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .CallBase();

            var engine = mockEngine.Object.Unlock();

            var topic = "MyTopic";
            var bootstrapServers = "MyServers";
            var start = new DateTime(2020, 9, 4, 14, 40, 16, DateTimeKind.Local).ToUniversalTime();

            IEnumerable<TopicPartitionTimestamp> t = engine.GetStartTimestamps(topic, bootstrapServers, start);
            var startTimestamps = t.ToArray();

            startTimestamps.Should().BeEmpty();

            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetAdminClient(bootstrapServers), Times.Once());

            mockAdminClient.Verify(m => m.GetMetadata(topic, It.IsAny<TimeSpan>()), Times.Once());
        }

        [Fact(DisplayName = "GetStartTimestamps protected method handles null TopicMetadata.Partitions")]
        public void GetStartTimestampsProtectedMethodHappyPath4()
        {
            var topics = new List<TopicMetadata>
            {
                new TopicMetadata("Topic1", null, null),
                new TopicMetadata("Topic2", null, null)
            };
            var metadata = new Metadata(new List<BrokerMetadata>(), topics, 0, "BrokerName");

            var mockAdminClient = new Mock<IAdminClient>();
            mockAdminClient.Setup(m => m.GetMetadata(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(metadata);

            var mockEngine = new Mock<DefaultReplayEngine>();
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetAdminClient(It.IsAny<string>()))
                .Returns(mockAdminClient.Object);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetStartTimestamps(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .CallBase();

            var engine = mockEngine.Object.Unlock();

            var topic = "MyTopic";
            var bootstrapServers = "MyServers";
            var start = new DateTime(2020, 9, 4, 14, 40, 16, DateTimeKind.Local).ToUniversalTime();

            IEnumerable<TopicPartitionTimestamp> t = engine.GetStartTimestamps(topic, bootstrapServers, start);
            var startTimestamps = t.ToArray();

            startTimestamps.Should().BeEmpty();

            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetAdminClient(bootstrapServers), Times.Once());

            mockAdminClient.Verify(m => m.GetMetadata(topic, It.IsAny<TimeSpan>()), Times.Once());
        }

        [Fact(DisplayName = "GetStartTimestamps protected method excludes when TopicMetadata has error")]
        public void GetStartTimestampsProtectedMethodHappyPath5()
        {
            var topic1Partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, Array.Empty<int>(), Array.Empty<int>(), null),
                new PartitionMetadata(1, 0, Array.Empty<int>(), Array.Empty<int>(), null)
            };
            var topic2Partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(2, 0, Array.Empty<int>(), Array.Empty<int>(), null),
                new PartitionMetadata(3, 0, Array.Empty<int>(), Array.Empty<int>(), null)
            };
            var topics = new List<TopicMetadata>
            {
                new TopicMetadata("Topic1", topic1Partitions, new Error(ErrorCode.Unknown)),
                new TopicMetadata("Topic2", topic2Partitions, new Error(ErrorCode.Unknown))
            };
            var metadata = new Metadata(new List<BrokerMetadata>(), topics, 0, "BrokerName");

            var mockAdminClient = new Mock<IAdminClient>();
            mockAdminClient.Setup(m => m.GetMetadata(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(metadata);

            var mockEngine = new Mock<DefaultReplayEngine>();
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetAdminClient(It.IsAny<string>()))
                .Returns(mockAdminClient.Object);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetStartTimestamps(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .CallBase();

            var engine = mockEngine.Object.Unlock();

            var topic = "MyTopic";
            var bootstrapServers = "MyServers";
            var start = new DateTime(2020, 9, 4, 14, 40, 16, DateTimeKind.Local).ToUniversalTime();

            IEnumerable<TopicPartitionTimestamp> t = engine.GetStartTimestamps(topic, bootstrapServers, start);
            var startTimestamps = t.ToArray();

            startTimestamps.Should().BeEmpty();

            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetAdminClient(bootstrapServers), Times.Once());

            mockAdminClient.Verify(m => m.GetMetadata(topic, It.IsAny<TimeSpan>()), Times.Once());
        }

        [Fact(DisplayName = "GetStartTimestamps protected method excludes when PartitionMetadata has error")]
        public void GetStartTimestampsProtectedMethodHappyPath6()
        {
            var topic1Partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(0, 0, Array.Empty<int>(), Array.Empty<int>(), new Error(ErrorCode.Unknown)),
                new PartitionMetadata(1, 0, Array.Empty<int>(), Array.Empty<int>(), new Error(ErrorCode.Unknown))
            };
            var topic2Partitions = new List<PartitionMetadata>
            {
                new PartitionMetadata(2, 0, Array.Empty<int>(), Array.Empty<int>(), new Error(ErrorCode.Unknown)),
                new PartitionMetadata(3, 0, Array.Empty<int>(), Array.Empty<int>(), new Error(ErrorCode.Unknown))
            };
            var topics = new List<TopicMetadata>
            {
                new TopicMetadata("Topic1", topic1Partitions, null),
                new TopicMetadata("Topic2", topic2Partitions, null)
            };
            var metadata = new Metadata(new List<BrokerMetadata>(), topics, 0, "BrokerName");

            var mockAdminClient = new Mock<IAdminClient>();
            mockAdminClient.Setup(m => m.GetMetadata(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(metadata);

            var mockEngine = new Mock<DefaultReplayEngine>();
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetAdminClient(It.IsAny<string>()))
                .Returns(mockAdminClient.Object);
            mockEngine.Protected().As<IProtected>()
                .Setup(m => m.GetStartTimestamps(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .CallBase();

            var engine = mockEngine.Object.Unlock();

            var topic = "MyTopic";
            var bootstrapServers = "MyServers";
            var start = new DateTime(2020, 9, 4, 14, 40, 16, DateTimeKind.Local).ToUniversalTime();

            IEnumerable<TopicPartitionTimestamp> t = engine.GetStartTimestamps(topic, bootstrapServers, start);
            var startTimestamps = t.ToArray();

            startTimestamps.Should().BeEmpty();

            mockEngine.Protected().As<IProtected>()
                .Verify(m => m.GetAdminClient(bootstrapServers), Times.Once());

            mockAdminClient.Verify(m => m.GetMetadata(topic, It.IsAny<TimeSpan>()), Times.Once());
        }

        [Fact(DisplayName = "Replay protected method does not invoke callback when consumer returns null result")]
        public async Task ReplayProtectedMethodHappyPath1()
        {
            var receivedMessages = new List<IReceiverMessage>();

            var mockConsumer = new Mock<IConsumer<string, byte[]>>();

            var startOffsets = new List<TopicPartitionOffset>
            {
                new TopicPartitionOffset("Topic1", new Partition(0), new Offset(4)),
                new TopicPartitionOffset("Topic1", new Partition(1), new Offset(5)),
                new TopicPartitionOffset("Topic2", new Partition(2), new Offset(6)),
                new TopicPartitionOffset("Topic2", new Partition(3), new Offset(7))
            };
            var endTimestamp = new DateTime(2020, 9, 4, 15, 35, 10, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback = message =>
            {
                receivedMessages.Add(message);
                return Task.CompletedTask;
            };
            var enableAutoOffsetStore = true;

            var engine = new DefaultReplayEngine().Unlock();

            await engine.Replay(mockConsumer.Object, startOffsets, endTimestamp, callback, enableAutoOffsetStore);

            mockConsumer.Verify(m => m.Assign(startOffsets), Times.Once());

            receivedMessages.Should().BeEmpty();
        }

        [Fact(DisplayName = "Replay protected method does not invoke callback when result timestamp is after endTimestamp")]
        public async Task ReplayProtectedMethodHappyPath2()
        {
            var receivedMessages = new List<IReceiverMessage>();

            var tomorrow = DateTime.UtcNow.AddDays(1);

            ConsumeResult<string, byte[]> FutureResult(string topic, int partition, long offset) =>
                new ConsumeResult<string, byte[]>()
                {
                    Message = new Message<string, byte[]>
                    {
                        Timestamp = new Timestamp(tomorrow),
                        Headers = new Headers(),
                        Value = Encoding.UTF8.GetBytes("Hello, world!")
                    },
                    TopicPartitionOffset = new TopicPartitionOffset(topic, new Partition(partition), new Offset(offset))
                };

            var result1 = FutureResult("Topic1", 0, 4);
            var result2 = FutureResult("Topic1", 1, 5);
            var result3 = FutureResult("Topic2", 2, 6);
            var result4 = FutureResult("Topic2", 3, 7);

            var mockConsumer = new Mock<IConsumer<string, byte[]>>();
            mockConsumer.SetupSequence(m => m.Consume(It.IsAny<TimeSpan>()))
                .Returns(result1)
                .Returns(result2)
                .Returns(result3)
                .Returns(result4);

            var startOffsets = new List<TopicPartitionOffset>
            {
                result1.TopicPartitionOffset,
                result2.TopicPartitionOffset,
                result3.TopicPartitionOffset,
                result4.TopicPartitionOffset
            };
            var endTimestamp = new DateTime(2020, 9, 4, 15, 35, 10, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback = message =>
            {
                receivedMessages.Add(message);
                return Task.CompletedTask;
            };
            var enableAutoOffsetStore = true;

            var engine = new DefaultReplayEngine().Unlock();

            await engine.Replay(mockConsumer.Object, startOffsets, endTimestamp, callback, enableAutoOffsetStore);

            mockConsumer.Verify(m => m.Assign(startOffsets), Times.Once());

            receivedMessages.Should().BeEmpty();
        }

        [Fact(DisplayName = "Replay protected method invokes callback for each result before endTimestamp")]
        public async Task ReplayProtectedMethodHappyPath3()
        {
            var receivedMessages = new List<IReceiverMessage>();

            var yesterday = DateTime.UtcNow.AddDays(-1);

            ConsumeResult<string, byte[]> PastResult(string message, string topic, int partition, long offset) =>
                new ConsumeResult<string, byte[]>()
                {
                    Message = new Message<string, byte[]>
                    {
                        Timestamp = new Timestamp(yesterday),
                        Headers = new Headers(),
                        Value = Encoding.UTF8.GetBytes(message)
                    },
                    TopicPartitionOffset = new TopicPartitionOffset(topic, new Partition(partition), new Offset(offset))
                };

            var result1 = PastResult("a", "Topic1", 0, 4);
            var result2 = PastResult("b", "Topic1", 1, 5);
            var result3 = PastResult("c", "Topic2", 2, 6);
            var result4 = PastResult("d", "Topic2", 3, 7);

            var mockConsumer = new Mock<IConsumer<string, byte[]>>();
            mockConsumer.SetupSequence(m => m.Consume(It.IsAny<TimeSpan>()))
                .Returns(result1)
                .Returns(result2)
                .Returns(result3)
                .Returns(result4)
                .Returns((ConsumeResult<string, byte[]>)null);

            var startOffsets = new List<TopicPartitionOffset>
            {
                result1.TopicPartitionOffset,
                result2.TopicPartitionOffset,
                result3.TopicPartitionOffset,
                result4.TopicPartitionOffset
            };
            var endTimestamp = new DateTime(2020, 9, 4, 15, 35, 10, DateTimeKind.Local).ToUniversalTime();
            Func<IReceiverMessage, Task> callback = message =>
            {
                receivedMessages.Add(message);
                return Task.CompletedTask;
            };
            var enableAutoOffsetStore = true;

            var engine = new DefaultReplayEngine().Unlock();

            await engine.Replay(mockConsumer.Object, startOffsets, endTimestamp, callback, enableAutoOffsetStore);

            mockConsumer.Verify(m => m.Assign(startOffsets), Times.Once());

            receivedMessages.Should().Contain(m => m.StringPayload == "a")
                .Which.Should().BeOfType<KafkaReceiverMessage>()
                .Which.Result.Should().BeSameAs(result1);
            receivedMessages.Should().Contain(m => m.StringPayload == "b")
                .Which.Should().BeOfType<KafkaReceiverMessage>()
                .Which.Result.Should().BeSameAs(result2);
            receivedMessages.Should().Contain(m => m.StringPayload == "c")
                .Which.Should().BeOfType<KafkaReceiverMessage>()
                .Which.Result.Should().BeSameAs(result3);
            receivedMessages.Should().Contain(m => m.StringPayload == "d")
                .Which.Should().BeOfType<KafkaReceiverMessage>()
                .Which.Result.Should().BeSameAs(result4);
        }

        public interface IProtected
        {
            DateTime GetUtcNow();
            IConsumer<string, byte[]> GetConsumer(string bootstrapServers, bool enableAutoOffsetStore, AutoOffsetReset autoOffsetReset);
            IEnumerable<TopicPartitionTimestamp> GetStartTimestamps(string topic, string bootstrapServers, DateTime start);
            IAdminClient GetAdminClient(string bootstrapServers);
            Task Replay(IConsumer<string, byte[]> consumer, List<TopicPartitionOffset> startOffsets, DateTime endTimestamp, Func<IReceiverMessage, Task> callback, bool enableAutoOffsetStore);
        }
    }
}

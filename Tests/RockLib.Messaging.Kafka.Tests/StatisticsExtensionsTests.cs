using System;
using FluentAssertions;
using Moq;
using RockLib.Dynamic;
using Xunit;

namespace RockLib.Messaging.Kafka.Tests
{
    public static class StatisticsExtensionsTests
    {
        [Fact]
        public static void CallAddStatisticsEmittedHandlerForReceiverWithNullTarget()
        {
            Action action = () => ((IReceiver)null!).AddStatisticsEmittedHandler((sender, s) => { });
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact]
        public static void CallAddStatisticsEmittedHandlerForReceiverWithNull()
        {
            using var receiver = new KafkaReceiver("NAME", "TOPIC", "GROUPID", "SERVERS");
            Action action = () => receiver.AddStatisticsEmittedHandler(null!);
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact]
        public static void CallAddStatisticsEmittedHandlerForReceiverForNonKafkaReceiver()
        {
            Action action = () => new Mock<IReceiver>().Object.AddStatisticsEmittedHandler((sender, s) => { });
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public static void CallAddStatisticsEmittedHandlerForReceiver()
        {
            using var receiver = new KafkaReceiver("NAME", "TOPIC", "GROUPID", "SERVERS");
            var statsData = "STATS!";
            var callCount = 0;

            void Handler(object? sender, string stats)
            {
                sender.Should().BeSameAs(receiver);
                stats.Should().Be(statsData);
                callCount++;
            }

            receiver.AddStatisticsEmittedHandler(Handler);
            receiver.Unlock().OnStatisticsEmitted(null, statsData);
            
            callCount.Should().Be(1, "Event handler should have been called");
        }
        
        [Fact]
        public static void CallAddStatisticsEmittedHandlerForSenderWithNullTarget()
        {
            Action action = () => ((ISender)null!).AddStatisticsEmittedHandler((sender, s) => { });
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact]
        public static void CallAddStatisticsEmittedHandlerForSenderWithNull()
        {
            using var sender = new KafkaSender("NAME", "TOPIC", 1, "SERVERS");
            Action action = () => sender.AddStatisticsEmittedHandler(null!);
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact]
        public static void CallAddStatisticsEmittedHandlerForSenderForNonKafkaSender()
        {
            Action action = () => new Mock<ISender>().Object.AddStatisticsEmittedHandler((sender, s) => { });
            action.Should().Throw<ArgumentException>();
        }
        
        [Fact]
        public static void CallAddStatisticsEmittedHandlerForSender()
        {
            using var sender = new KafkaSender("NAME", "TOPIC", 1, "SERVERS");
            var statsData = "STATS!";
            var callCount = 0;

            void Handler(object? s, string stats)
            {
                s.Should().BeSameAs(sender);
                stats.Should().Be(statsData);
                callCount++;
            }

            sender.AddStatisticsEmittedHandler(Handler);
            sender.Unlock().OnStatisticsEmitted(null, statsData);
            
            callCount.Should().Be(1, "Event handler should have been called");
        }
    }
}
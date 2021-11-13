using System;
using FluentAssertions;
using Moq;
using RockLib.Dynamic;
using Xunit;

namespace RockLib.Messaging.Kafka.Tests
{
    public class StatisticsExtensionsTests
    {
        [Fact(DisplayName = "Throws argument null exception for null receiver")]
        public void ReceiverAddStatisticsEmittedHandlerSadPath1()
        {
            Action action = () => ((IReceiver)null).AddStatisticsEmittedHandler((sender, s) => { });
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact(DisplayName = "Throws argument null exception for null event handler")]
        public void ReceiverAddStatisticsEmittedHandlerSadPath2()
        {
            var receiver = new KafkaReceiver("NAME", "TOPIC", "GROUPID", "SERVERS");
            Action action = () => receiver.AddStatisticsEmittedHandler(null);
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact(DisplayName = "Throws argument exception for non-KafkaReceiver")]
        public void ReceiverAddStatisticsEmittedHandlerSadPath3()
        {
            Action action = () => new Mock<IReceiver>().Object.AddStatisticsEmittedHandler((sender, s) => { });
            action.Should().Throw<ArgumentException>();
        }

        [Fact(DisplayName = "Adds event handler to given KafkaReceiver")]
        public void ReceiverAddStatisticsEmittedHandlerHappyPath()
        {
            var receiver = new KafkaReceiver("NAME", "TOPIC", "GROUPID", "SERVERS");
            var statsData = "STATS!";
            var callCount = 0;

            void Handler(object sender, string stats)
            {
                sender.Should().BeSameAs(receiver);
                stats.Should().Be(statsData);
                callCount++;
            }

            receiver.AddStatisticsEmittedHandler(Handler);
            receiver.Unlock().OnStatisticsEmitted(null, statsData);
            
            callCount.Should().Be(1, "Event handler should have been called");
        }
        
        [Fact(DisplayName = "Throws argument null exception for null sender")]
        public void SenderAddStatisticsEmittedHandlerSadPath1()
        {
            Action action = () => ((ISender)null).AddStatisticsEmittedHandler((sender, s) => { });
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact(DisplayName = "Throws argument null exception for null event handler")]
        public void SenderAddStatisticsEmittedHandlerSadPath2()
        {
            var sender = new KafkaSender("NAME", "TOPIC", 1, "SERVERS");
            Action action = () => sender.AddStatisticsEmittedHandler(null);
            action.Should().Throw<ArgumentNullException>();
        }
        
        [Fact(DisplayName = "Throws argument exception for non-KafkaSender")]
        public void SenderAddStatisticsEmittedHandlerSadPath3()
        {
            Action action = () => new Mock<ISender>().Object.AddStatisticsEmittedHandler((sender, s) => { });
            action.Should().Throw<ArgumentException>();
        }
        
        [Fact(DisplayName = "Adds event handler to given KafkaSender")]
        public void SenderAddStatisticsEmittedHandlerHappyPath()
        {
            var sender = new KafkaSender("NAME", "TOPIC", 1, "SERVERS");
            var statsData = "STATS!";
            var callCount = 0;

            void Handler(object s, string stats)
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
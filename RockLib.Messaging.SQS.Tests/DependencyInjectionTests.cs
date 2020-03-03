using Amazon;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RockLib.Messaging.DependencyInjection;
using Xunit;

namespace RockLib.Messaging.SQS.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void SQSSenderTest()
        {
            var services = new ServiceCollection();

            services.AddSQSSender("mySender", options =>
            {
                options.QueueUrl = "http://example.com";
                options.Region = "us-west-2";
                options.MessageGroupId = "myMessageGroupId";
            });

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var sqsSender = sender.Should().BeOfType<SQSSender>().Subject;

            sqsSender.Name.Should().Be("mySender");
            sqsSender.QueueUrl.Should().Be("http://example.com");
            sqsSender.SQSClient.Config.RegionEndpoint.Should().Be(RegionEndpoint.USWest2);
            sqsSender.MessageGroupId.Should().Be("myMessageGroupId");
        }

        [Fact]
        public void SQSReceiverTest()
        {
            var services = new ServiceCollection();

            services.AddSQSReceiver("myReceiver", options =>
            {
                options.QueueUrl = "http://example.com";
                options.Region = "us-west-2";
                options.MaxMessages = 5;
                options.AutoAcknowledge = false;
                options.WaitTimeSeconds = 123;
                options.UnpackSNS = true;
            });

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            var sqsReceiver = receiver.Should().BeOfType<SQSReceiver>().Subject;

            sqsReceiver.Name.Should().Be("myReceiver");
            sqsReceiver.QueueUrl.Should().Be("http://example.com");
            sqsReceiver.SQSClient.Config.RegionEndpoint.Should().Be(RegionEndpoint.USWest2);
            sqsReceiver.MaxMessages.Should().Be(5);
            sqsReceiver.AutoAcknwoledge.Should().BeFalse();
            sqsReceiver.WaitTimeSeconds.Should().Be(123);
            sqsReceiver.UnpackSNS.Should().BeTrue();
        }
    }
}

using Amazon;
using Amazon.SQS;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RockLib.Dynamic;
using RockLib.Messaging.DependencyInjection;
using System;
using Xunit;

namespace RockLib.Messaging.SQS.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void SQSSenderTest1()
        {
            var services = new ServiceCollection();
            services.Configure<SQSSenderOptions>(options => { });

            services.AddSQSSender("mySender", options =>
            {
                options.QueueUrl = "http://example.com";
                options.Region = "us-west-2";
                options.MessageGroupId = "myMessageGroupId";
            }, false);

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var sqsSender = sender.Should().BeOfType<SQSSender>().Subject;

            sqsSender.Name.Should().Be("mySender");
            sqsSender.QueueUrl.Should().Be("http://example.com");
            sqsSender.SQSClient.Config.RegionEndpoint.Should().Be(RegionEndpoint.USWest2);
            sqsSender.MessageGroupId.Should().Be("myMessageGroupId");
        }

        [Fact]
        public void SQSSenderTest2()
        {
            var reloadingSenderType = Type.GetType("RockLib.Messaging.DependencyInjection.ReloadingSender`1, RockLib.Messaging", true)
               .MakeGenericType(typeof(SQSSenderOptions));

            var services = new ServiceCollection();
            services.Configure<SQSSenderOptions>(options => { });

            services.AddSQSSender("mySender", options =>
            {
                options.QueueUrl = "http://example.com";
                options.Region = "us-west-2";
                options.MessageGroupId = "myMessageGroupId";
            }, true);

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType(reloadingSenderType);

            var sqsSender = (SQSSender)sender.Unlock().Sender;

            sqsSender.Name.Should().Be("mySender");
            sqsSender.QueueUrl.Should().Be("http://example.com");
            sqsSender.SQSClient.Config.RegionEndpoint.Should().Be(RegionEndpoint.USWest2);
            sqsSender.MessageGroupId.Should().Be("myMessageGroupId");
        }

        [Fact(DisplayName = "Should register SQS sender with registered SQS client when available")]
        public void SQSSenderRegisteredClientTest()
        {
            var services = new ServiceCollection();

            var sqsClient = new AmazonSQSClient(RegionEndpoint.USEast2);
            services.AddSingleton<IAmazonSQS>(sqsClient);

            services.AddSQSSender("mySender", options =>
            {
                options.QueueUrl = "http://example.com";
            });

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var sqsSender = sender.Should().BeOfType<SQSSender>().Subject;

            sqsSender.Name.Should().Be("mySender");
            sqsSender.SQSClient.Should().BeSameAs(sqsClient);
        }

        [Fact]
        public void SQSReceiverTest1()
        {
            var services = new ServiceCollection();
            services.Configure<SQSReceiverOptions>(options => { });

            services.AddSQSReceiver("myReceiver", options =>
            {
                options.QueueUrl = "http://example.com";
                options.Region = "us-west-2";
                options.MaxMessages = 5;
                options.AutoAcknowledge = false;
                options.WaitTimeSeconds = 123;
                options.UnpackSNS = true;
            }, false);

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

        [Fact]
        public void SQSReceiverTest2()
        {
            var reloadingReceiverType = Type.GetType("RockLib.Messaging.DependencyInjection.ReloadingReceiver`1, RockLib.Messaging", true)
               .MakeGenericType(typeof(SQSReceiverOptions));

            var services = new ServiceCollection();
            services.Configure<SQSReceiverOptions>(options => { });

            services.AddSQSReceiver("myReceiver", options =>
            {
                options.QueueUrl = "http://example.com";
                options.Region = "us-west-2";
                options.MaxMessages = 5;
                options.AutoAcknowledge = false;
                options.WaitTimeSeconds = 123;
                options.UnpackSNS = true;
            }, true);

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType(reloadingReceiverType);

            var sqsReceiver = (SQSReceiver)receiver.Unlock().Receiver;

            sqsReceiver.Name.Should().Be("myReceiver");
            sqsReceiver.QueueUrl.Should().Be("http://example.com");
            sqsReceiver.SQSClient.Config.RegionEndpoint.Should().Be(RegionEndpoint.USWest2);
            sqsReceiver.MaxMessages.Should().Be(5);
            sqsReceiver.AutoAcknwoledge.Should().BeFalse();
            sqsReceiver.WaitTimeSeconds.Should().Be(123);
            sqsReceiver.UnpackSNS.Should().BeTrue();
        }

        [Fact(DisplayName = "Should register SQS receiver with registered SQS client when available")]
        public void SQSReceiverRegisteredClientTest()
        {
            var services = new ServiceCollection();

            var sqsClient = new AmazonSQSClient(RegionEndpoint.USEast2);
            services.AddSingleton<IAmazonSQS>(sqsClient);

            services.AddSQSReceiver("myReceiver", options =>
            {
                options.QueueUrl = "http://example.com";
            });

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            var sqsReceiver = receiver.Should().BeOfType<SQSReceiver>().Subject;

            sqsReceiver.Name.Should().Be("myReceiver");
            sqsReceiver.SQSClient.Should().BeSameAs(sqsClient);
        }
    }
}

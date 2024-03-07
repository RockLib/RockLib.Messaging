using Amazon;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RockLib.Dynamic;
using RockLib.Messaging.DependencyInjection;
using System;
using Xunit;

namespace RockLib.Messaging.SNS.Tests
{
    public static class DependencyInjectionTests
    {
        [Fact]
        public static void SendViaSNSSender()
        {
            var services = new ServiceCollection();
            services.Configure<SNSSenderOptions>(options => { });

            services.AddSNSSender("mySender", options =>
            {
                options.TopicArn = "myTopicArn";
                options.Region = "us-west-2";
            }, false);

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var snsSender = sender.Should().BeOfType<SNSSender>().Subject;

            snsSender.Name.Should().Be("mySender");
            snsSender.TopicArn.Should().Be("myTopicArn");
            snsSender.SnsClient.Config.RegionEndpoint.Should().Be(RegionEndpoint.USWest2);
        }

        [Fact]
        public static void SendViaReloadingSender()
        {
            var reloadingSenderType = Type.GetType("RockLib.Messaging.DependencyInjection.ReloadingSender`1, RockLib.Messaging", true)!
               .MakeGenericType(typeof(SNSSenderOptions));

            var services = new ServiceCollection();
            services.Configure<SNSSenderOptions>(options => { });

            services.AddSNSSender("mySender", options =>
            {
                options.TopicArn = "myTopicArn";
                options.Region = "us-west-2";
            }, true);

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType(reloadingSenderType);

            var snsSender = (SNSSender)sender.Unlock().Sender;

            snsSender.Name.Should().Be("mySender");
            snsSender.TopicArn.Should().Be("myTopicArn");
            snsSender.SnsClient.Config.RegionEndpoint.Should().Be(RegionEndpoint.USWest2);
        }
    }
}
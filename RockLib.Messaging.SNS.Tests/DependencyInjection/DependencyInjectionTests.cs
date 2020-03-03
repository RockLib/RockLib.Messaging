using Amazon;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RockLib.Messaging.DependencyInjection;
using Xunit;

namespace RockLib.Messaging.SNS.Tests.DependencyInjection
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void SNSSenderTest()
        {
            var services = new ServiceCollection();

            services.AddSNSSender("mySender", options =>
            {
                options.TopicArn = "myTopicArn";
                options.Region = "us-west-2";
            });

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var snsSender = sender.Should().BeOfType<SNSSender>().Subject;

            snsSender.Name.Should().Be("mySender");
            snsSender.TopicArn.Should().Be("myTopicArn");
            snsSender.SnsClient.Config.RegionEndpoint.Should().Be(RegionEndpoint.USWest2);
        }
    }
}

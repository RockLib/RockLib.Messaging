using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RockLib.Messaging.DependencyInjection;
using Xunit;

namespace RockLib.Messaging.NamedPipes.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void NamedPipeSenderTest()
        {
            var services = new ServiceCollection();

            services.AddNamedPipeSender("mySender", options => options.PipeName = "myPipeName");

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var namedPipeSender = sender.Should().BeOfType<NamedPipeSender>().Subject;

            namedPipeSender.Name.Should().Be("mySender");
            namedPipeSender.PipeName.Should().Be("myPipeName");
        }

        [Fact]
        public void NamedPipeReceiverTest()
        {
            var services = new ServiceCollection();

            services.AddNamedPipeReceiver("myReceiver", options => options.PipeName = "myPipeName");

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            var namedPipeReceiver = receiver.Should().BeOfType<NamedPipeReceiver>().Subject;

            namedPipeReceiver.Name.Should().Be("myReceiver");
            namedPipeReceiver.PipeName.Should().Be("myPipeName");
        }
    }
}

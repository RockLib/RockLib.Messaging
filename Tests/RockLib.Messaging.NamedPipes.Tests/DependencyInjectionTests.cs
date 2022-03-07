using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RockLib.Dynamic;
using RockLib.Messaging.DependencyInjection;
using System;
using Xunit;

namespace RockLib.Messaging.NamedPipes.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void NamedPipeSenderTest1()
        {
            var services = new ServiceCollection();
            services.Configure<NamedPipeOptions>(options => { });

            services.AddNamedPipeSender("mySender", options => options.PipeName = "myPipeName", false);

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            var namedPipeSender = sender.Should().BeOfType<NamedPipeSender>().Subject;

            namedPipeSender.Name.Should().Be("mySender");
            namedPipeSender.PipeName.Should().Be("myPipeName");
        }

        [Fact]
        public void NamedPipeSenderTest2()
        {
            var reloadingSenderType = Type.GetType("RockLib.Messaging.DependencyInjection.ReloadingSender`1, RockLib.Messaging", true)!
                .MakeGenericType(typeof(NamedPipeOptions));

            var services = new ServiceCollection();
            services.Configure<NamedPipeOptions>(options => { });

            services.AddNamedPipeSender("mySender", options => options.PipeName = "myPipeName", true);

            var serviceProvider = services.BuildServiceProvider();

            var sender = serviceProvider.GetRequiredService<ISender>();

            sender.Should().BeOfType(reloadingSenderType);

            var namedPipeSender = (NamedPipeSender)sender.Unlock().Sender;

            namedPipeSender.Name.Should().Be("mySender");
            namedPipeSender.PipeName.Should().Be("myPipeName");
        }

        [Fact]
        public void NamedPipeReceiverTest1()
        {
            var services = new ServiceCollection();
            services.Configure<NamedPipeOptions>(options => { });

            services.AddNamedPipeReceiver("myReceiver", options => options.PipeName = "myPipeName", false);

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            var namedPipeReceiver = receiver.Should().BeOfType<NamedPipeReceiver>().Subject;

            namedPipeReceiver.Name.Should().Be("myReceiver");
            namedPipeReceiver.PipeName.Should().Be("myPipeName");
        }

        [Fact]
        public void NamedPipeReceiverTest2()
        {
            var reloadingReceiverType = Type.GetType("RockLib.Messaging.DependencyInjection.ReloadingReceiver`1, RockLib.Messaging", true)!
                .MakeGenericType(typeof(NamedPipeOptions));

            var services = new ServiceCollection();
            services.Configure<NamedPipeOptions>(options => { });

            services.AddNamedPipeReceiver("myReceiver", options => options.PipeName = "myPipeName", true);

            var serviceProvider = services.BuildServiceProvider();

            var receiver = serviceProvider.GetRequiredService<IReceiver>();

            receiver.Should().BeOfType(reloadingReceiverType);
            
            var namedPipeReceiver = (NamedPipeReceiver)receiver.Unlock().Receiver;

            namedPipeReceiver.Name.Should().Be("myReceiver");
            namedPipeReceiver.PipeName.Should().Be("myPipeName");
        }
    }
}

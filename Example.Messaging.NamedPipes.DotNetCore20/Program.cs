using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RockLib.Messaging.DependencyInjection;
using System;

namespace Example.Messaging.NamedPipes.DotNetCore20
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Note that the host builder is configured in a slightly different way depending on
            // the service being run. This is to demonstrate the different ways of registering and
            // consuming named pipe senders and receivers.

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

            Console.WriteLine("Select the service to run:");
            Console.WriteLine($"1) {nameof(DataSendingService)}");
            Console.WriteLine($"2) {nameof(CommandSendingService)}");
            Console.WriteLine($"3) {nameof(ReceivingService)}");
            Console.WriteLine($"4) {nameof(SingleMessageService)}");
            Console.Write(">");

            while (true)
            {
                switch (Console.ReadKey(true).KeyChar)
                {
                    case '1':
                        Console.WriteLine('1');
                        return hostBuilder.ConfigureServices((hostContext, services) =>
                        {
                            // Configuring a sender's NamedPipeOptions programmatically:
                            services.Configure<NamedPipeOptions>("DataSender", options => options.PipeName = "data_pipe");
                            services.AddNamedPipeSender("DataSender");

                            // Since only one ISender is registered, the constructor of DataSendingService
                            // has an ISender parameter. If more than one ISender was registered, the service
                            // would have a SenderLookup parameter instead.
                            services.AddHostedService<DataSendingService>();
                        });
                    case '2':
                        Console.WriteLine('2');
                        return hostBuilder.ConfigureServices(services =>
                        {
                            // Configuring a sender's NamedPipeOptions programmatically:
                            services.AddNamedPipeSender("CommandSender", options => options.PipeName = "command_pipe");

                            // Since only one ISender is registered, the constructor of CommandSendingService
                            // has an ISender parameter. If more than one ISender was registered, the service
                            // would have a SenderLookup parameter instead.
                            services.AddHostedService<CommandSendingService>();
                        });
                    case '3':
                        Console.WriteLine('3');
                        return hostBuilder.ConfigureServices((hostContext, services) =>
                        {
                            // Configuring a receiver's NamedPipeOptions from configuration (appsettings.json in this case):
                            IConfigurationSection dataSettings = hostContext.Configuration.GetSection("DataSettings");
                            services.Configure<NamedPipeOptions>("DataReceiver", dataSettings);
                            services.AddNamedPipeReceiver("DataReceiver");

                            // Configuring a receiver's NamedPipeOptions configuration (appsettings.json in this case):
                            IConfigurationSection commandSettings = hostContext.Configuration.GetSection("CommandSettings");
                            services.Configure<NamedPipeOptions>("CommandReceiver", commandSettings);
                            services.AddNamedPipeReceiver("CommandReceiver", options => options.PipeName = "command_pipe");

                            // Since more than one IReceiver is registered, the constructor of ReceivingService
                            // has a ReceiverLookup lookup parameter, allowing it to retreive receivers by name.
                            // If only one IReceiver was registered, the service would have an IReceiver parameter
                            // instead.
                            services.AddHostedService<ReceivingService>();
                        });
                    case '4':
                        Console.WriteLine('4');
                        return hostBuilder.ConfigureServices(services =>
                        {
                            // Adding a sender/receiver by name using MessagingScenarioFactory:
                            services.AddSender("ExampleSender");
                            services.AddReceiver("ExampleReceiver");

                            // Since only one ISender and one IReceiver are registered, the constructor of
                            // SingleMessageService has an ISender parameter and an IReceiver parameter.
                            // If more than one ISender or IReceiver was registered, the service would have
                            // a SenderLookup or ReceiverLookup parameter instead.
                            services.AddHostedService<SingleMessageService>();
                        });
                }
            }
        }
    }
}

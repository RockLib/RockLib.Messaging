using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime.Internal.Util;
using Example.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RockLib.Messaging.DependencyInjection;
using System;
using System.Linq;

namespace Example.Messaging.SQS.DotNetCore31
{
    class Program
    {
        static void Main(string[] args)
        {
            EnsureAwsCredentials();

            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            // Note that the host builder is configured in a slightly different way depending on
            // the service being run. This is to demonstrate the different ways of registering and
            // consuming SQS senders and receivers.

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
                        return hostBuilder.ConfigureServices(services =>
                        {
                            // Configuring a sender's SQSSenderOptions programmatically:
                            services.AddSQSSender("DataSender", options =>
                                options.QueueUrl = new Uri("https://sqs.{region}.amazonaws.com/{account-id}/{data-queue-name}"));

                            // Since only one ISender is registered, the constructor of DataSendingService
                            // has an ISender parameter. If more than one ISender was registered, the service
                            // would have a SenderLookup parameter instead.
                            services.AddHostedService<DataSendingService>();
                        });
                    case '2':
                        Console.WriteLine('2');
                        return hostBuilder.ConfigureServices(services =>
                        {
                            // Configuring a sender's SQSSenderOptions programmatically:
                            services.AddSQSSender("CommandSender", options =>
                                options.QueueUrl = new Uri("https://sqs.{region}.amazonaws.com/{account-id}/{command-queue-name}"));

                            // Since only one ISender is registered, the constructor of CommandSendingService
                            // has an ISender parameter. If more than one ISender was registered, the service
                            // would have a SenderLookup parameter instead.
                            services.AddHostedService<CommandSendingService>();
                        });
                    case '3':
                        Console.WriteLine('3');
                        return hostBuilder.ConfigureServices((hostContext, services) =>
                        {
                            // Configuring a receiver's SQSReceiverOptions from configuration (appsettings.json in this case):
                            IConfiguration dataConfig = hostContext.Configuration.GetSection("DataSettings");
                            services.Configure<SQSReceiverOptions>("DataReceiver", dataConfig);
                            services.AddSQSReceiver("DataReceiver");

                            // Configuring a receiver's SQSReceiverOptions configuration (appsettings.json in this case):
                            IConfiguration commandConfig = hostContext.Configuration.GetSection("CommandSettings");
                            services.Configure<SQSReceiverOptions>("CommandReceiver", commandConfig);
                            services.AddSQSReceiver("CommandReceiver");

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
                            // Adding a sender/receiver by name using MessagingScenarioFactory (which is defined
                            // by the 'RockLib.Messaging' configuration section):
                            services.AddSender("SingleMessageSender");
                            services.AddReceiver("SingleMessageReceiver");

                            // Since only one ISender and one IReceiver are registered, the constructor of
                            // SingleMessageService has an ISender parameter and an IReceiver parameter.
                            // If more than one ISender or IReceiver was registered, the service would have
                            // a SenderLookup or ReceiverLookup parameter instead.
                            services.AddHostedService<SingleMessageService>();
                        });
                }
            }
        }

        const string _defaultProfile = "default";

        private static void EnsureAwsCredentials()
        {
            // http://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-creds.html

            var credentialsFile = new NetSDKCredentialsFile();

            if (!credentialsFile.TryGetProfile(_defaultProfile, out var _))
                RegisterProfileFromUser(credentialsFile);
        }

        private static void RegisterProfileFromUser(NetSDKCredentialsFile credentialsFile = null)
        {
            credentialsFile ??= new NetSDKCredentialsFile();

            Console.Write("Access Key>");
            var accessKey = Console.ReadLine();

            Console.Write("Secret Key>");
            var secretKey = Console.ReadLine();

            RegionEndpoint region;

            var regions = RegionEndpoint.EnumerableAllRegions.OrderBy(r => r.SystemName).ToList();
            for (int i = 0; i < regions.Count; i++)
                Console.WriteLine($"{i}) {regions[i]}");

            Console.Write("Region>");
            while (true)
            {
                var line = Console.ReadLine();
                if (int.TryParse(line, out var index) && index >= 0 && index < regions.Count)
                {
                    region = regions[index];
                    break;
                }

                var blank = new string(' ', line.Length);
                Console.SetCursorPosition("Region>".Length, Console.CursorTop - 1);
                Console.Write(blank);
                Console.SetCursorPosition("Region>".Length, Console.CursorTop);
            }

            var options = new CredentialProfileOptions
            {
                AccessKey = accessKey,
                SecretKey = secretKey
            };

            var profile = new CredentialProfile(_defaultProfile, options) { Region = region };
            credentialsFile.RegisterProfile(profile);
        }
    }
}

﻿using System;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using RockLib.Messaging;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using RockLib.Messaging.DependencyInjection;

namespace Example.Messaging.SNS.DotNetCore20
{
    class Program
    {
        const string DefaultProfile = "default";

        static async Task Main(string[] args)
        {
            EnsureAwsCredentials();

            void DisplayPrompt()
            {
                Console.WriteLine("1) Start the SNS Sender and start sending and receiving messages.");
                Console.WriteLine("2) Re-enter AWS credentials.");
                Console.WriteLine("q) Quit");
                Console.WriteLine("Tip: Start multiple instances of this app and have them send and receive to each other.");
                Console.Write("selection >");
            }

            DisplayPrompt();

            var services = new ServiceCollection();

            services.AddSNSSender("Sender1", options =>
            {
                options.Region = "us-east-1"; // TODO: Set Region
                options.TopicArn = "topic:arn"; // TODO: Set Topic Arn
            });
            services.AddSQSReceiver("Receiver1", options =>
            {
                options.AutoAcknowledge = false;
                options.Region = "us-east-1"; // TODO: Set Region
                options.QueueUrl = new Uri("http://aws.com"); // TODO: Set Topic Url
                options.UnpackSNS = true;
            });
            services.AddSQSReceiver("Receiver2", options =>
            {
                options.AutoAcknowledge = false;
                options.Region = "us-east-1"; // TODO: Set Region
                options.QueueUrl = new Uri("http://aws.com"); // TODO: Set Topic Url
                options.UnpackSNS = true;
            });
            services.AddSQSReceiver("Receiver3", options =>
            {
                options.AutoAcknowledge = false;
                options.Region = "us-east-1"; // TODO: Set Region
                options.QueueUrl = new Uri("http://aws.com"); // TODO: Set Topic Url
                options.UnpackSNS = true;
            });

            var serviceProvider = services.BuildServiceProvider();

            while (true)
            {
                var c = Console.ReadKey(true).KeyChar;
                switch (c)
                {
                    case '1':
                        Console.WriteLine(c);
                        RunSender(serviceProvider, RunReceivers(serviceProvider)).Wait();
                        return;
                    case '2':
                        Console.WriteLine(c);
                        RegisterProfileFromUser();
                        DisplayPrompt();
                        break;
                    case 'q':
                    case 'Q':
                        Console.WriteLine(c);
                        return;
                }
            }
        }

        private static async Task RunSender(IServiceProvider serviceProvider, List<IReceiver> receivers)
        {
            using (var sender = serviceProvider.GetRequiredService<ISender>())
            {
                while (true)
                {
                    Console.WriteLine($"Enter a message for sender '{sender.Name}'. Add headers as a trailing JSON object. Leave blank to quit.");
                    Console.Write("message> ");
                    string message;
                    if ((message = Console.ReadLine()) == "")
                        break;

                    if (TryExtractHeaders(ref message, out var headers))
                        await sender.SendAsync(new SenderMessage(message) { Headers = headers });
                    else
                        await sender.SendAsync(message);

                    Thread.Sleep(1000);
                }

                foreach (var receiver in receivers)
                    receiver.Dispose();
            }
        }

        private static List<IReceiver> RunReceivers(IServiceProvider serviceProvider)
        {
            var receivers = serviceProvider.GetServices<IReceiver>().ToList();
            foreach (var receiver in receivers)
                receiver.Start(m => HandleMessage(m, receiver.Name));
            return receivers;
        }

        private static async Task HandleMessage(IReceiverMessage m, string name)
        {
            var builder = new StringBuilder();
            foreach (var header in m.Headers)
                builder.AppendLine($"{name} - {header.Key}: {header.Value}");

            builder.AppendLine($"{name} - {m.StringPayload}");
            builder.AppendLine();
            Console.WriteLine(builder);

            await m.AcknowledgeAsync();
        }

        private static void EnsureAwsCredentials()
        {
            // http://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-creds.html

            var credentialsFile = new NetSDKCredentialsFile();
            if (!credentialsFile.TryGetProfile(DefaultProfile, out _))
            {
                RegisterProfileFromUser(credentialsFile);
            }
        }

        private static void RegisterProfileFromUser(NetSDKCredentialsFile credentialsFile = null)
        {
            credentialsFile = credentialsFile ?? new NetSDKCredentialsFile();

            Console.Write("Access Key > ");
            var accessKey = Console.ReadLine();

            Console.Write("Secret Key > ");
            var secretKey = Console.ReadLine();

            RegionEndpoint region;

            var regions = RegionEndpoint.EnumerableAllRegions.OrderBy(r => r.SystemName).ToList();
            for (var i = 0; i < regions.Count; i++)
                Console.WriteLine($"{i}) {regions[i]}");

            Console.Write("Region > ");
            while (true)
            {
                var line = Console.ReadLine();
                if (int.TryParse(line, out var index) && index >= 0 && index < regions.Count)
                {
                    region = regions[index];
                    break;
                }

                var blank = new string(' ', line.Length);
                Console.SetCursorPosition("Region > ".Length, Console.CursorTop - 1);
                Console.Write(blank);
                Console.SetCursorPosition("Region > ".Length, Console.CursorTop);
            }

            var options = new CredentialProfileOptions
            {
                AccessKey = accessKey,
                SecretKey = secretKey
            };

            var profile = new CredentialProfile(DefaultProfile, options) { Region = region };
            credentialsFile.RegisterProfile(profile);
        }

        private static bool TryExtractHeaders(ref string message, out IDictionary<string, object> headers)
        {
            headers = new Dictionary<string, object>();
            var trim = message.TrimEnd();
            if (trim.Length <= 1 || trim[trim.Length - 1] != '}' || trim[trim.Length - 2] == '\\')
                return false;

            var stack = new Stack<char>();
            stack.Push('}');
            for (var i = trim.Length - 2; i >= 0; i--)
            {
                switch (trim[i])
                {
                    case '{':
                        stack.Pop();

                        if (stack.Count != 0)
                            continue;
                        JObject json;
                        try
                        {
                            json = JObject.Parse(message.Substring(i));
                            message = message.Substring(0, i);
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                        string GetString(object value)
                        {
                            switch (value)
                            {
                                case string stringValue:
                                    return stringValue;
                                case bool boolValue:
                                    return boolValue ? "true" : "false";
                                default:
                                    return value.ToString();
                            }
                        }
                        foreach (var field in json)
                            if (field.Value is JValue value)
                                headers.Add(field.Key, GetString(value.Value));
                        return true;
                    case '}':
                        stack.Push('}');
                        break;
                }
            }

            return false;
        }
    }
}

﻿using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RockLib.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Example.Messaging.RabbitMQ.DotNetCore20
{
    class Program
    {
        static async Task Main(string[] args)
        {
            EnsureQueueExists();

            Console.WriteLine("Make a selection:");
            Console.WriteLine("1) Create an ISender and prompt for messages.");
            Console.WriteLine("2) Create an IReceiver and listen for messages.");
            Console.WriteLine("3) Create an ISender and an IReceiver and send one message from one to the other.");
            Console.WriteLine("q) Quit");
            Console.WriteLine("Tip: Start multiple instances of this app and have them send and receive to each other.");
            Console.Write("selection>");

            while (true)
            {
                var c = Console.ReadKey(true).KeyChar;
                switch (c)
                {
                    case '1':
                        Console.WriteLine(c);
                        await RunSender();
                        return;
                    case '2':
                        Console.WriteLine(c);
                        RunReceiver();
                        return;
                    case '3':
                        Console.WriteLine(c);
                        await SendAndReceiveOneMessage();
                        return;
                    case 'q':
                        Console.WriteLine(c);
                        return;
                }
            }
        }

        private static void EnsureQueueExists()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
                channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false);
        }

        private static async Task RunSender()
        {
            using (var sender = MessagingScenarioFactory.CreateSender("Sender1"))
            {
                Console.WriteLine($"Enter a message for sender '{sender.Name}'. Add headers as a trailing JSON object. Leave blank to quit.");
                string message;
                while (true)
                {
                    Console.Write("message>");
                    if ((message = Console.ReadLine()) == "")
                        return;

                    if (TryExtractHeaders(ref message, out var headers))
                        await sender.SendAsync(new SenderMessage(message) { Headers = headers });
                    else
                        await sender.SendAsync(message);
                }
            }
        }

        private static void RunReceiver()
        {
            using (var receiver = MessagingScenarioFactory.CreateReceiver("Receiver1"))
            {
                receiver.Error += (obj, args) =>
                {
                    // Do something when the receiver ecounters an error
                    throw new Exception($"Error in the receiver: {args.Exception.Message}");
                };

                receiver.Start(async m =>
                {
                    foreach (var header in m.Headers)
                        Console.WriteLine($"{header.Key}: {header.Value}");

                    Console.WriteLine(m.StringPayload);

                    // Always acknowledge, reject, or rollback a received message.
                    await m.AcknowledgeAsync();
                });
                Console.WriteLine($"Receiving messages from receiver '{receiver.Name}'. Press <enter> to quit.");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            }
        }

        private static async Task SendAndReceiveOneMessage()
        {
            // Use a wait handle to pause the main thread while waiting for the message to be received.
            var waitHandle = new AutoResetEvent(false);

            var sender = MessagingScenarioFactory.CreateSender("Sender1");
            var receiver = MessagingScenarioFactory.CreateReceiver("Receiver1");

            receiver.Error += (obj, args) =>
            {
                // Do something when the receiver ecounters an error
                throw new Exception($"Error in the receiver: {args.Exception.Message}");
            };

            receiver.Start(async m =>
            {
                var message = m.StringPayload;

                Console.WriteLine($"Message received: {message}");

                // Always acknowledge, reject, or rollback a received message.
                await m.AcknowledgeAsync();

                waitHandle.Set();
            });

            await sender.SendAsync($"RabbitMQ test message from {typeof(Program).FullName}");

            waitHandle.WaitOne();

            receiver.Dispose();
            sender.Dispose();
            waitHandle.Dispose();

            Console.Write("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static bool TryExtractHeaders(ref string message, out IDictionary<string, object> headers)
        {
            headers = new Dictionary<string, object>();
            var trim = message.TrimEnd();
            if (trim.Length > 1 && trim[trim.Length - 1] == '}' && trim[trim.Length - 2] != '\\')
            {
                var stack = new Stack<char>();
                stack.Push('}');
                for (int i = trim.Length - 2; i >= 0; i--)
                {
                    if (trim[i] == '{')
                    {
                        stack.Pop();

                        if (stack.Count == 0)
                        {
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
                        }
                    }
                    else if (trim[i] == '}')
                        stack.Push('}');
                }
            }

            return false;
        }
    }
}

using System;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using RockLib.Messaging;
using System.Threading;

namespace Example.Messaging.SQS.DotNetFramework451
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // The AWS credentials should be provide via a profile or other more secure means. This is only for example.
            // See http://http://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-creds.html
            var options = new CredentialProfileOptions
            {
                AccessKey = "AKIAJWXCD7ZCUGRQYCMA",
                SecretKey = "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"
            };
            var profile = new CredentialProfile("default", options) {Region = RegionEndpoint.USWest2};
            new NetSDKCredentialsFile().RegisterProfile(profile);

            // Use a wait handle to pause the main thread while waiting for the message to be received.
            var waitHandle = new AutoResetEvent(false);

            var producer = MessagingScenarioFactory.CreateQueueProducer("queue");
            var consumer = MessagingScenarioFactory.CreateQueueConsumer("queue");

            consumer.Start();
            consumer.MessageReceived += (sender, eventArgs) =>
            {
                var eventArgsMessage = eventArgs.Message;
                var message = eventArgsMessage.GetStringValue();

                Console.WriteLine($"Message: {message}");

                waitHandle.Set();
            };
            consumer.Start();

            producer.Send($"SQS test message from {typeof(Program).FullName}");

            waitHandle.WaitOne();

            consumer.Dispose();
            producer.Dispose();
            waitHandle.Dispose();

            Console.Write("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
using System;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using RockLib.Messaging;

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

            var producer = MessagingScenarioFactory.CreateQueueProducer("queue");
            var consumer = MessagingScenarioFactory.CreateQueueConsumer("queue");

            consumer.Start();
            consumer.MessageReceived += (sender, eventArgs) =>
            {
                var eventArgsMessage = eventArgs.Message;
                var message = eventArgsMessage.GetStringValue();

                Console.WriteLine($"Message: {message}");
            };

            producer.Send("Test SQS Message!");
        }
    }
}
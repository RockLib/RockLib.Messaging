using System;
using System.Threading;
using NUnit.Framework;

namespace Rock.Messaging.RabbitMQ.IntegrationTests
{
    public class ConfigTests
    {
        [Test]
        public void WhenSetViaConfigDefaultMessagingScenarioFactoryIsAnInstanceOfRabbitMessagingScenarioFactory()
        {
            Assert.That(MessagingScenarioFactory.Current, Is.InstanceOf<RabbitMessagingScenarioFactory>());
        }

        [Test]
        public void IntegrationHappyPath()
        {
            var numberOfMessagesToSend = 1000;
            var numberOfReceivedMessages = 0;
            var i = MessagingScenarioFactory.CreateQueueConsumer("test1");
            i.MessageReceived += delegate(object sender, MessageReceivedEventArgs args)
            {
                numberOfReceivedMessages++;
                Console.WriteLine("1: "+ args.Message.GetStringValue()); };
            var i2 = MessagingScenarioFactory.CreateQueueConsumer("test1");
            i2.MessageReceived += delegate(object sender, MessageReceivedEventArgs args)
            {
                numberOfReceivedMessages++;
                Thread.Sleep(100);
                Console.WriteLine("2 : " + args.Message.GetStringValue());
            };
            i.Start();
            i2.Start();
            var producer = MessagingScenarioFactory.CreateQueueProducer("test1");

            for (int j = 0; j < numberOfMessagesToSend; j++)
            {
                producer.Send(j.ToString());
            }
            Thread.Sleep(10000);
            Assert.That(numberOfReceivedMessages == numberOfMessagesToSend, numberOfReceivedMessages.ToString());
        }
    }
}

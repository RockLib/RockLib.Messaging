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
            var i = MessagingScenarioFactory.CreateQueueConsumer("test1");
            i.MessageReceived += delegate(object sender, MessageReceivedEventArgs args) { Console.WriteLine("1: "+ args.Message.GetStringValue()); };
            var i2 = MessagingScenarioFactory.CreateQueueConsumer("test1");
            i2.MessageReceived += delegate (object sender, MessageReceivedEventArgs args) { Thread.Sleep(100); Console.WriteLine("2 : "+args.Message.GetStringValue()); };
            i.Start();
            i2.Start();
            var producer = MessagingScenarioFactory.CreateQueueProducer("test1");
            var rand = new Random();
            for (int j = 0; j < 1000; j++)
            {
                producer.Send(j.ToString());
            }
            Thread.Sleep(10000);
        }
    }
}

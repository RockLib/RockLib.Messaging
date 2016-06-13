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
            var consumer1 = MessagingScenarioFactory.CreateQueueConsumer("test1");
            consumer1.MessageReceived += delegate(object sender, MessageReceivedEventArgs args)
            {
                numberOfReceivedMessages++;
                Console.WriteLine("1: "+ args.Message.GetStringValue());
            };
            var consumer2 = MessagingScenarioFactory.CreateQueueConsumer("test1");
            consumer2.MessageReceived += delegate(object sender, MessageReceivedEventArgs args)
            {
                numberOfReceivedMessages++;
                Thread.Sleep(100);
                Console.WriteLine("2: " + args.Message.GetStringValue());
            };
            consumer2.Start();
            consumer1.Start();
            
            var producer = MessagingScenarioFactory.CreateQueueProducer("test1");

            for (int j = 0; j < numberOfMessagesToSend; j++)
            {
                producer.Send(j.ToString());
            }
            Thread.Sleep(10000);

            Assert.That(numberOfReceivedMessages, Is.EqualTo(numberOfMessagesToSend), numberOfReceivedMessages.ToString());
        }

        /// <summary>
        /// These configs don't set up exchanges or bind queues to them.
        /// Run IntegrationHappyPath first, or set up the exchange/queues by hand locally. 
        /// </summary>
        [Test]
        public void MinimalistConfigsShouldBehaveAsExpected()
        {
            var consumer = MessagingScenarioFactory.CreateQueueConsumer("validReceiver");
            bool roundTrip = false;
            consumer.MessageReceived += (sender, args) => roundTrip = true;
            consumer.Start();
            var producer = MessagingScenarioFactory.CreateQueueProducer("validSender");
            for (int i = 0; i < 10; i++)
            {
                producer.Send("TestMessagePleaseIgnore");
            }
            
            Thread.Sleep(100);

            Assert.That(roundTrip);
        }
    }
}

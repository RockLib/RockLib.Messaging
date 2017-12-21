using RockLib.Messaging;
using System;
using System.Threading;

namespace Example.Messaging.DotNetFramework451
{
    class Program
    {
        static void Main(string[] args)
        {
            // Use a wait handle to pause the main thread while waiting for the message to be received.
            var waitHandle = new AutoResetEvent(false);

            var namedPipeProducer = MessagingScenarioFactory.CreateQueueProducer("Pipe1");
            var namedPipeConsumer = MessagingScenarioFactory.CreateQueueConsumer("Pipe1");

            namedPipeConsumer.MessageReceived += (sender, eventArgs) =>
            {
                var eventArgsMessage = eventArgs.Message;
                var message = eventArgsMessage.GetStringValue();

                Console.WriteLine($"Message: {message}");

                waitHandle.Set();
            };
            namedPipeConsumer.Start();

            namedPipeProducer.Send($"Named pipe test message from {typeof(Program).FullName}");

            waitHandle.WaitOne();

            namedPipeConsumer.Dispose();
            namedPipeProducer.Dispose();
            waitHandle.Dispose();

            Console.Write("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}

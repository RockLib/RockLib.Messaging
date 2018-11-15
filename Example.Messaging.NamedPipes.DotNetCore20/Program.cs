using RockLib.Messaging;
using System;
using System.Threading;

namespace Example.Messaging.DotNetCore20
{
    class Program
    {
        static void Main(string[] args)
        {
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
                        RunSender();
                        return;
                    case '2':
                        Console.WriteLine(c);
                        RunReceiver();
                        return;
                    case '3':
                        Console.WriteLine(c);
                        SendAndReceiveOneMessage();
                        return;
                    case 'q':
                        Console.WriteLine(c);
                        return;
                }
            }
        }

        private static void RunSender()
        {
            using (var sender = MessagingScenarioFactory.CreateSender("Sender1"))
            {
                Console.WriteLine($"Enter a message for sender '{sender.Name}'. Leave blank to quit.");
                string message;
                while (true)
                {
                    Console.Write("message>");
                    if ((message = Console.ReadLine()) == "")
                        return;
                    sender.Send(message);
                }
            }
        }

        private static void RunReceiver()
        {
            using (var receiver = MessagingScenarioFactory.CreateReceiver("Receiver1"))
            {
                receiver.Start(m => Console.WriteLine(m.StringPayload));
                Console.WriteLine($"Receiving messages from receiver '{receiver.Name}'. Press <enter> to quit.");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
            }
        }

        private static void SendAndReceiveOneMessage()
        {
            // Use a wait handle to pause the main thread while waiting for the message to be received.
            var waitHandle = new AutoResetEvent(false);

            var namedPipeProducer = MessagingScenarioFactory.CreateSender("Sender1");
            var namedPipeConsumer = MessagingScenarioFactory.CreateReceiver("Receiver1");

            namedPipeConsumer.Start(m =>
            {
                var message = m.StringPayload;

                Console.WriteLine($"Message received: {message}");

                waitHandle.Set();
            });

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

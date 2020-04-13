using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example.Common
{
    public abstract class SendingService : IHostedService
    {
        private readonly Thread _senderThread;

        protected SendingService(ISender sender)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _senderThread = new Thread(SendMessages) { IsBackground = true };
        }

        public ISender Sender { get; }

        protected abstract string Prompt { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _senderThread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Sender.Dispose();
            return Task.CompletedTask;
        }

        private void SendMessages()
        {
            // Wait a bit to let the startup logs get written to console. Otherwise, logs
            // get written after the prompt and it's a weird experience for the user.
            Thread.Sleep(500);

            Console.WriteLine(Prompt);
            while (true)
            {
                Console.Write(">");
                ReadAndSendMessage();
            }
        }

        private void ReadAndSendMessage()
        {
            string message = ReadLine();
            if (message is object)
                Sender.Send(message);
        }

        protected virtual string ReadLine() => Console.ReadLine();
    }
}

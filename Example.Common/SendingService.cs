using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example.Common
{
    public abstract class SendingService : IHostedService
    {
        private readonly ISender _sender;
        private readonly Thread _senderThread;

        protected SendingService(ISender sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _senderThread = new Thread(SendMessages) { IsBackground = true };
        }

        protected abstract string Prompt { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _senderThread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _sender.Dispose();
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
                string message = Console.ReadLine();
                _sender.Send(message);
            }
        }
    }
}

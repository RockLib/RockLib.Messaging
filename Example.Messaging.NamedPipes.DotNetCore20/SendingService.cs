using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example.Messaging.NamedPipes.DotNetCore20
{
    abstract class SendingService : IHostedService
    {
        private readonly ISender _sender;
        private readonly string _prompt;
        private readonly Thread _senderThread;

        protected SendingService(ISender sender, string prompt)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
            _senderThread = new Thread(SendMessages) { IsBackground = true };
        }

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

            Console.WriteLine(_prompt);
            while (true)
            {
                Console.Write(">");
                string message = Console.ReadLine();
                _sender.Send(message);
            }
        }
    }
}

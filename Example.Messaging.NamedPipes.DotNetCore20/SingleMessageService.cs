using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example.Messaging.NamedPipes.DotNetCore20
{
    class SingleMessageService : IHostedService
    {
        private readonly ISender _sender;
        private readonly IReceiver _receiver;

        public SingleMessageService(ISender sender, IReceiver receiver)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _receiver.Start(OnMessageReceived);
            ThreadPool.QueueUserWorkItem(SendMessage, _sender, false);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static Task OnMessageReceived(IReceiverMessage message)
        {
            Console.WriteLine($"Received message: '{message.StringPayload}'");
            return message.AcknowledgeAsync();
        }

        private static void SendMessage(ISender sender)
        {
            Thread.Sleep(1000);
            sender.Send($"[{DateTime.Now:G}] Example message");
        }
    }
}

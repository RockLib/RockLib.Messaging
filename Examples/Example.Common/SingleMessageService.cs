using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example.Common
{
    public class SingleMessageService : IHostedService
    {
        public SingleMessageService(ISender sender, IReceiver receiver)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }

        public ISender Sender { get; }

        public IReceiver Receiver { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Receiver.Start(OnMessageReceived);
            ThreadPool.QueueUserWorkItem(async _ => await WaitAndSendMessage());

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private Task OnMessageReceived(IReceiverMessage message)
        {
            Console.WriteLine($"Received message: '{message.StringPayload}'");
            return message.AcknowledgeAsync();
        }

        private async Task WaitAndSendMessage()
        {
            Wait();
            await Sender.SendAsync($"[{Now:G}] Example message");
        }

        protected virtual void Wait() => Thread.Sleep(1000);

        protected virtual DateTime Now => DateTime.Now;
    }
}

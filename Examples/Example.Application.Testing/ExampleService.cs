using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
    public class ExampleService : IHostedService
    {
        public ExampleService(IReceiver receiver, IDatabase database)
        {
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public IReceiver Receiver { get; }

        public IDatabase Database { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Receiver.Start(OnMessageReceived);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Receiver.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(IReceiverMessage message)
        {
            if (message.Headers.TryGetValue("operation", out string operation))
            {
                if (operation == "create")
                {
                    await Database.CreateAsync(message.StringPayload);
                    await message.AcknowledgeAsync();
                }
                else if (operation == "update")
                {
                    await Database.UpdateAsync(message.StringPayload);
                    await message.AcknowledgeAsync();
                }
                else if (operation == "delete")
                {
                    await Database.DeleteAsync(message.StringPayload);
                    await message.AcknowledgeAsync();
                }
                else
                {
                    // TODO: Send error log - invalid message
                    await message.RejectAsync();
                }
            }
            else
            {
                // TODO: Send error log - invalid message
                await message.RejectAsync();
            }
        }
    }
}
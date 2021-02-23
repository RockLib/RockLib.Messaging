using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using RockLib.Messaging.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Example.Common
{
    public class ReceivingService : IHostedService
    {
        public const string DataReceiverName = "DataReceiver";
        public const string CommandReceiverName = "CommandReceiver";

        public ReceivingService(ReceiverLookup receiverLookup)
        {
            if (receiverLookup == null)
                throw new ArgumentNullException(nameof(receiverLookup));

            DataReceiver = receiverLookup(DataReceiverName)
                ?? throw new ArgumentException("Must have an IReceiver registered with the name 'DataReceiver'.", nameof(receiverLookup));

            CommandReceiver = receiverLookup(CommandReceiverName)
                ?? throw new ArgumentException("Must have an IReceiver registered with the name 'CommandReceiver'.", nameof(receiverLookup));
        }

        public IReceiver DataReceiver { get; }

        public IReceiver CommandReceiver { get; }

        public Casing Casing { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DataReceiver.Start(DataReceivedAsync);
            CommandReceiver.Start(CommandReceivedAsync);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            DataReceiver.Dispose();
            CommandReceiver.Dispose();
            return Task.CompletedTask;
        }

        public Task DataReceivedAsync(IReceiverMessage message)
        {
            WriteLine(FormatMessage(message.StringPayload));
            return message.AcknowledgeAsync();
        }

        protected virtual void WriteLine(string message) => Console.WriteLine(message);

        protected virtual string FormatMessage(string payload) => Casing switch
        {
            Casing.UPPER => payload.ToUpperInvariant(),
            Casing.lower => payload.ToLowerInvariant(),
            Casing.SpOnGeBoB => new string(payload.Select((c, i) => i % 2 == 0 ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c)).ToArray()),
            _ => payload
        };

        private Task CommandReceivedAsync(IReceiverMessage message)
        {
            if (Enum.TryParse(message.StringPayload, ignoreCase: true, out Casing casing))
                Casing = casing;
            return message.AcknowledgeAsync();
        }
    }
}

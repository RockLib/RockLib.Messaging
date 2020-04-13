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

        private readonly IReceiver _dataReceiver;
        private readonly IReceiver _commandReceiver;

        private Casing _casing;

        public ReceivingService(ReceiverLookup receiverLookup)
        {
            if (receiverLookup == null)
                throw new ArgumentNullException(nameof(receiverLookup));

            _dataReceiver = receiverLookup(DataReceiverName)
                ?? throw new ArgumentException("Must have an IReceiver registered with the name 'DataReceiver'.", nameof(receiverLookup));

            _commandReceiver = receiverLookup(CommandReceiverName)
                ?? throw new ArgumentException("Must have an IReceiver registered with the name 'CommandReceiver'.", nameof(receiverLookup));
        }

        public static ReceivingService Create(IReceiver dataReceiver, IReceiver commandReceiver) =>
            new ReceivingService(name => name switch
            {
                DataReceiverName => dataReceiver,
                CommandReceiverName => commandReceiver,
                _ => null
            });

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _dataReceiver.Start(DataReceived);
            _commandReceiver.Start(CommandReceived);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _dataReceiver.Dispose();
            _commandReceiver.Dispose();
            return Task.CompletedTask;
        }

        private Task DataReceived(IReceiverMessage message)
        {
            Console.WriteLine(FormatMessage(message.StringPayload));
            return message.AcknowledgeAsync();
        }

        private string FormatMessage(string payload) => _casing switch
        {
            Casing.UPPER => payload.ToUpperInvariant(),
            Casing.lower => payload.ToLowerInvariant(),
            Casing.SpOnGeBoB => new string(payload.Select((c, i) => i % 2 == 0 ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c)).ToArray()),
            _ => payload
        };

        private Task CommandReceived(IReceiverMessage message)
        {
            if (Enum.TryParse(message.StringPayload, ignoreCase: true, out Casing casing))
                _casing = casing;
            return message.AcknowledgeAsync();
        }
    }
}

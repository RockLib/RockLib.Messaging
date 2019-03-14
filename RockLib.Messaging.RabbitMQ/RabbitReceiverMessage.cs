using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.RabbitMQ
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="RabbitReceiver"/>
    /// class.
    /// </summary>
    public sealed class RabbitReceiverMessage : ReceiverMessage
    {
        private static readonly Task CompletedTask = Task.FromResult(0);

        private BasicDeliverEventArgs _args;
        private IModel _channel;

        internal RabbitReceiverMessage(BasicDeliverEventArgs args, IModel channel)
             : base(() => args.Body)
        {
            _args = args;
            _channel = channel;
        }

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            foreach (var header in _args.BasicProperties.Headers)
                headers.Add(header);
        }

        /// <inheritdoc />
        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken)
        {
            _channel.BasicAck(_args.DeliveryTag, false);
            return CompletedTask;
        }

        /// <inheritdoc />
        protected override Task RollbackMessageAsync(CancellationToken cancellationToken)
        {
            _channel.BasicNack(_args.DeliveryTag, false, true);
            return CompletedTask;
        }

        /// <inheritdoc />
        protected override Task RejectMessageAsync(CancellationToken cancellationToken)
        {
            _channel.BasicNack(_args.DeliveryTag, false, false);
            return CompletedTask;
        }
    }
}
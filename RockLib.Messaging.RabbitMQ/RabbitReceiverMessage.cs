using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace RockLib.Messaging.RabbitMQ
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="RabbitReceiver"/>
    /// class.
    /// </summary>
    public sealed class RabbitReceiverMessage : ReceiverMessage
    {
        private static readonly Task CompletedTask = Task.FromResult(0);

        private readonly BasicDeliverEventArgs _args;
        private readonly IModel _channel;
        private readonly bool _autoAck;

        internal RabbitReceiverMessage(BasicDeliverEventArgs args, IModel channel, bool autoAck)
             : base(() => args.Body.ToArray())
        {
            _args = args;
            _channel = channel;
            _autoAck = autoAck;
        }

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            if (_args.BasicProperties?.Headers == null)
                return;

            foreach (var header in _args.BasicProperties.Headers)
            {
                if (header.Value is byte[] binary)
                    headers.Add(header.Key, Encoding.UTF8.GetString(binary));
                else
                    headers.Add(header);
            }
        }

        /// <inheritdoc />
        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken)
        {
            if (!_autoAck)
                _channel.BasicAck(_args.DeliveryTag, false);
            return CompletedTask;
        }

        /// <inheritdoc />
        protected override Task RollbackMessageAsync(CancellationToken cancellationToken)
        {
            if (!_autoAck)
                _channel.BasicNack(_args.DeliveryTag, false, true);
            return CompletedTask;
        }

        /// <inheritdoc />
        protected override Task RejectMessageAsync(CancellationToken cancellationToken)
        {
            if (!_autoAck)
                _channel.BasicNack(_args.DeliveryTag, false, false);
            return CompletedTask;
        }
    }
}
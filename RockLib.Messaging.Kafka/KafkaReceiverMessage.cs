using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    public class KafkaReceiverMessage : ReceiverMessage
    {
        private readonly Consumer<Ignore, string> _consumer;

        internal KafkaReceiverMessage(Consumer<Ignore, string> consumer, ConsumeResult<Ignore, string> result)
            : base(() => result.Value)
        {
            _consumer = consumer;
            Result = result;
        }

        public ConsumeResult<Ignore, string> Result { get; }

        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            if (Result.Headers != null)
                foreach (var header in Result.Headers)
                    headers.Add(header.Key, Encoding.UTF8.GetString(header.Value));
        }

        protected override Task RejectMessageAsync(CancellationToken cancellationToken) => CommitAsync(cancellationToken);

        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken) => CommitAsync(cancellationToken);

        protected override Task RollbackMessageAsync(CancellationToken cancellationToken) => Tasks.CompletedTask;

        private Task CommitAsync(CancellationToken cancellationToken)
        {
            try
            {
                _consumer.Commit(Result, cancellationToken);
                return Tasks.CompletedTask;
            }
            catch (Exception ex)
            {
                return Tasks.FromException(ex);
            }
        }
    }
}

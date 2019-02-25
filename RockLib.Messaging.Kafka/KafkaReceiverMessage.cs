using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="KafkaReceiver"/>
    /// class.
    /// </summary>
    public class KafkaReceiverMessage : ReceiverMessage
    {
        internal KafkaReceiverMessage(IConsumer<Ignore, string> consumer, ConsumeResult<Ignore, string> result)
            : base(() => result.Value)
        {
            Consumer = consumer;
            Result = result;
        }

        /// <summary>
        /// Gets the <see cref="Consumer{TKey, TValue}"/> that received the message.
        /// </summary>
        public IConsumer<Ignore, string> Consumer { get; }

        /// <summary>
        /// Gets the actual Kafka message that was received.
        /// </summary>
        public ConsumeResult<Ignore, string> Result { get; }

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            if (Result.Headers != null)
                foreach (var header in Result.Headers)
                    headers.Add(header.Key, Encoding.UTF8.GetString(header.Value));
        }

        /// <inheritdoc />
        protected override Task RejectMessageAsync(CancellationToken cancellationToken) => CommitAsync();

        /// <inheritdoc />
        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken) => CommitAsync();

        /// <inheritdoc />
        protected override Task RollbackMessageAsync(CancellationToken cancellationToken) => Tasks.CompletedTask;

        private Task CommitAsync()
        {
            try
            {
                Consumer.Commit(Result);
                return Tasks.CompletedTask;
            }
            catch (Exception ex)
            {
                return Tasks.FromException(ex);
            }
        }
    }
}

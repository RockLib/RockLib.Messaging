using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RockLib.Messaging.Kafka.Constants;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="KafkaReceiver"/>
    /// class.
    /// </summary>
    public class KafkaReceiverMessage : ReceiverMessage
    {
        private readonly bool _enableAutoOffsetStore;

        internal KafkaReceiverMessage(IConsumer<string, byte[]> consumer, ConsumeResult<string, byte[]> result, bool enableAutoOffsetStore)
            : base(() => result.Message.Value)
        {
            Consumer = consumer;
            Result = result;
            _enableAutoOffsetStore = enableAutoOffsetStore;
        }

        /// <summary>
        /// Gets the <see cref="Consumer{TKey, TValue}"/> that received the message.
        /// </summary>
        public IConsumer<string, byte[]> Consumer { get; }

        /// <summary>
        /// Gets the actual Kafka message that was received.
        /// </summary>
        public ConsumeResult<string, byte[]> Result { get; }

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            if (Result.Message?.Key is string key)
                headers[KafkaKeyHeader] = key;

            if (Result.Message?.Headers != null)
                foreach (var header in Result.Message.Headers)
                    headers[header.Key] = Encoding.UTF8.GetString(header.GetValueBytes()); // TODO: Unconditionally using utf-8 might not be the right thing to do here.
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

                if (_enableAutoOffsetStore is false)
                    Consumer.StoreOffset(Result);

                return Tasks.CompletedTask;
            }
            catch (Exception ex)
            {
                return Tasks.FromException(ex);
            }
        }
    }
}

using System;
using Confluent.Kafka;
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
        private const int HeaderSize = sizeof(byte) + sizeof(int);
        
        private readonly bool _enableAutoOffsetStore;
        private readonly bool _containsSchemaId;

        internal KafkaReceiverMessage(IConsumer<string, byte[]> consumer, ConsumeResult<string, byte[]> result, bool enableAutoOffsetStore, bool containsSchemaId)
            : base(() => GetRawPayload(result.Message?.Value ?? new byte[0], containsSchemaId))
        {
            Consumer = consumer;
            Result = result;
            _enableAutoOffsetStore = enableAutoOffsetStore;
            _containsSchemaId = containsSchemaId;
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
                    headers[header.Key] = Encoding.UTF8.GetString(header.GetValueBytes());
        }

        private static byte[] GetRawPayload(byte[] payload, bool containsSchemaId)
        {
            if (containsSchemaId)
            {
#if NETCOREAPP5_0_OR_GREATER
                return payload[HeaderSize..];
#else
                var subset = new byte[payload.Length - HeaderSize];
                Array.Copy(payload, HeaderSize, subset, 0, subset.Length);
                return subset;
#endif
            }

            return payload;
        }

        /// <inheritdoc />
        protected override Task RejectMessageAsync(CancellationToken cancellationToken) => StoreOffsetAsync();

        /// <inheritdoc />
        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken) => StoreOffsetAsync();

        /// <inheritdoc />
        protected override Task RollbackMessageAsync(CancellationToken cancellationToken) => Tasks.CompletedTask;

        private Task StoreOffsetAsync()
        {
            if (_enableAutoOffsetStore is false)
                Consumer.StoreOffset(Result);

            return Tasks.CompletedTask;
        }
    }
}

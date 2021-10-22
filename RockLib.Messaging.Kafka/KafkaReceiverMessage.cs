using System;
using Confluent.Kafka;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

            if (TryGetSchemaId(Result.Message?.Value, _containsSchemaId, out var schemaId))
                headers[KafkaSchemaIdHeader] = schemaId;

            if (Result.Message?.Headers != null)
                foreach (var header in Result.Message.Headers)
                    headers[header.Key] = Encoding.UTF8.GetString(header.GetValueBytes());
        }

        private static byte[] GetRawPayload(byte[] payload, bool containsSchemaId)
        {
            if (!containsSchemaId) return payload;

#if NET5_0_OR_GREATER
            return payload[HeaderSize..];
#else
            var body = new byte[payload.Length - HeaderSize];
            Array.Copy(payload, HeaderSize, body, 0, body.Length);
            return body;
#endif
        }

        private static bool TryGetSchemaId(byte[] payload, bool containsSchemaId, out int schemaId)
        {
            if (!containsSchemaId || payload[0] != SchemaIdLeadingByte)
            {
                schemaId = -1;
                return false;
            }

            using (var ms = new MemoryStream(payload))
            using (var reader = new BinaryReader(ms))
            {
                reader.ReadByte(); //move past leading magic byte
                schemaId = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            }
            
            return true;
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

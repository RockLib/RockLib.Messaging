using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using RockLib.Messaging.Kafka.Exceptions;
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
            
            CheckPayloadLength(payload);
            
#if NET5_0_OR_GREATER
            return payload[HeaderSize..];
#else
            var body = new byte[payload.Length - HeaderSize];
            Array.Copy(payload, HeaderSize, body, 0, body.Length);
            return body;
#endif
        }

        private static void CheckPayloadLength(byte[] payload)
        {
            if (payload.Length <= HeaderSize)
            {
                throw new InvalidMessageException($"Expected payload greater than {HeaderSize} bytes but payload is {payload.Length} bytes");
            }
        }
        
        private static bool TryGetSchemaId(byte[] payload, bool containsSchemaId, out int schemaId)
        {
            if (!containsSchemaId)
            {
                schemaId = -1;
                return false;
            }

            CheckPayloadLength(payload);

            if (payload[0] != SchemaIdLeadingByte)
            {
                throw new InvalidMessageException($"Expected schema registry data frame. Magic byte was {payload[0]} instead of {SchemaIdLeadingByte}");
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

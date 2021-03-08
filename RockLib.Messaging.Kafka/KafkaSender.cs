using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RockLib.Messaging.Kafka.Constants;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that sends messages to Kafka.
    /// </summary>
    public class KafkaSender : ISender
    {
        private const byte SchemaIdLeadingByte = 0;
        private static readonly char[] _quoteChar = { '"' };

        private readonly Lazy<IProducer<string, byte[]>> _producer;

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaSender"/> class.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topic">The topic to produce messages to.</param>
        /// <param name="bootstrapServers">List of brokers as a CSV list of broker host or host:port.</param>
        /// <param name="messageTimeoutMs">
        /// Local message timeout. This value is only enforced locally and limits the time
        /// a produced message waits for successful delivery. A time of 0 is infinite. This
        /// is the maximum time librdkafka may use to deliver a message (including retries).
        /// Delivery error occurs when either the retry count or the message timeout are
        /// exceeded.
        /// </param>
        public KafkaSender(string name, string topic, string bootstrapServers, int messageTimeoutMs = 10000)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
            MessageTimeoutMs = messageTimeoutMs;

            var config = GetProducerConfig(bootstrapServers, messageTimeoutMs);

            var producerBuilder = new ProducerBuilder<string, byte[]>(config);
            producerBuilder.SetErrorHandler(OnError);

            _producer = new Lazy<IProducer<string, byte[]>>(() => producerBuilder.Build());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaSender"/> class.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topic">The topic to produce messages to.</param>
        /// <param name="producerConfig">The configuration used in creation of the Kafka producer.</param>
        public KafkaSender(string name, string topic, ProducerConfig producerConfig)
        {
            if (producerConfig is null)
                throw new ArgumentNullException(nameof(producerConfig));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            BootstrapServers = producerConfig.BootstrapServers;
            MessageTimeoutMs = producerConfig.MessageTimeoutMs;

            var producerBuilder = new ProducerBuilder<string, byte[]>(producerConfig);
            producerBuilder.SetErrorHandler(OnError);

            _producer = new Lazy<IProducer<string, byte[]>>(() => producerBuilder.Build());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaSender"/> class.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topic">The topic to produce messages to.</param>
        /// <param name="schemaId">The schema ID to have the broker validate messages against.</param>
        /// <param name="bootstrapServers">List of brokers as a CSV list of broker host or host:port.</param>
        /// <param name="messageTimeoutMs">
        /// Local message timeout. This value is only enforced locally and limits the time
        /// a produced message waits for successful delivery. A time of 0 is infinite. This
        /// is the maximum time librdkafka may use to deliver a message (including retries).
        /// Delivery error occurs when either the retry count or the message timeout are
        /// exceeded.
        /// </param>
        public KafkaSender(string name, string topic, int schemaId, string bootstrapServers, int messageTimeoutMs = 10000)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
            MessageTimeoutMs = messageTimeoutMs;

            if (schemaId <= 0)
                throw new ArgumentOutOfRangeException(nameof(schemaId), "Should be greater than 0");
            SchemaId = schemaId;

            var config = GetProducerConfig(bootstrapServers, messageTimeoutMs);

            var producerBuilder = new ProducerBuilder<string, byte[]>(config);
            producerBuilder.SetErrorHandler(OnError);

            _producer = new Lazy<IProducer<string, byte[]>>(() => producerBuilder.Build());
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="KafkaSender"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the topic to subscribe to.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// List of brokers as a CSV list of broker host or host:port.
        /// </summary>
        public string BootstrapServers { get; }

        /// <summary>
        /// Local message timeout. This value is only enforced locally and limits the time
        /// a produced message waits for successful delivery. A time of 0 is infinite. This
        /// is the maximum time librdkafka may use to deliver a message (including retries).
        /// Delivery error occurs when either the retry count or the message timeout are
        /// exceeded.
        /// </summary>
        public int? MessageTimeoutMs { get; }
        
        /// <summary>
        /// Gets the schema ID messages will be validated against.
        /// </summary>
        public int? SchemaId { get; }

        /// <summary>
        /// Gets the <see cref="IProducer{TKey, TValue}" /> for this instance of <see cref="KafkaSender"/>.
        /// </summary>
        public IProducer<string, byte[]> Producer { get { return _producer.Value; } }

        /// <summary>
        /// Occurs when an error happens on a background thread.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Determine if the message should include the schema ID for validation
        /// </summary>
        private bool ShouldIncludeSchemaId => SchemaId > 0;

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "Kafka";

            var kafkaMessage = new Message<string, byte[]> { Value = BuildMessagePayload(message) };

            if (message.Headers.TryGetValue(KafkaKeyHeader, out object objValue)
                && Serialize(objValue) is string kafkaKey)
            {
                kafkaMessage.Key = kafkaKey;
                message.Headers.Remove(KafkaKeyHeader);
            }

            if (message.Headers.Count > 0)
            {
                kafkaMessage.Headers = kafkaMessage.Headers ?? new Headers();
                foreach (var header in message.Headers)
                    if (Encode(Serialize(header.Value)) is byte[] headerValue)
                        kafkaMessage.Headers.Add(header.Key, headerValue);
            }

            return _producer.Value.ProduceAsync(Topic, kafkaMessage);
        }

        /// <summary>
        /// Flushes the producer and disposes it.
        /// </summary>
        public void Dispose()
        {
            if (_producer.IsValueCreated)
            {
                _producer.Value.Flush(TimeSpan.FromSeconds(10));
                _producer.Value.Dispose();
            }
        }

        private void OnError(IProducer<string, byte[]> producer, Error error) 
            => Error?.Invoke(this, new ErrorEventArgs(error.Reason, new KafkaException(error)));

        private static string Serialize(object value)
        {
            if (value is string stringValue)
                return stringValue;

            if (value is null)
                return null;

            return JsonConvert.SerializeObject(value).Trim(_quoteChar);
        }

        private static byte[] Encode(string value)
        {
            if (value is null)
                return null;

            return Encoding.UTF8.GetBytes(value);
        }

        internal static ProducerConfig GetProducerConfig(string bootstrapServers, int messageTimeoutMs)
        {
            return new ProducerConfig()
            {
                BootstrapServers = bootstrapServers,
                MessageTimeoutMs = messageTimeoutMs
            };
        }

        private byte[] BuildMessagePayload(SenderMessage message)
        {
            if (!ShouldIncludeSchemaId) return message.BinaryPayload;
            
            using (var memoryStream = new MemoryStream())
            using (var binaryWriter = new BinaryWriter(memoryStream))
            {
                memoryStream.WriteByte(SchemaIdLeadingByte);
                binaryWriter.Write(IPAddress.HostToNetworkOrder(SchemaId.Value));
                binaryWriter.Write(message.BinaryPayload);
                return memoryStream.ToArray();
            }
        }
    }
}

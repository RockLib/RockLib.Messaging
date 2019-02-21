using Confluent.Kafka;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that sends messages to Kafka.
    /// </summary>
    public class KafkaSender : ISender
    {
        private readonly Lazy<Producer<Null, string>> _producer;

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaSender"/> class.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="topic">
        /// The topic to subscribe to. A regex can be specified to subscribe to the set of
        /// all matching topics (which is updated as topics are added / removed from the
        /// cluster). A regex must be front anchored to be recognized as a regex. e.g. ^myregex
        /// </param>
        /// <param name="bootstrapServers">
        /// Initial list of brokers as a CSV list of broker host or host:port. The application
        /// may also use `rd_kafka_brokers_add()` to add brokers during runtime.
        /// </param>
        /// <param name="useBeginProduce">
        /// Whether to use the
        /// <see cref="Producer{TKey, TValue}.BeginProduce(string, Message{TKey, TValue}, Action{DeliveryReport{TKey, TValue}})"/>
        /// method to send messages. If false, the
        /// <see cref="Producer{TKey, TValue}.ProduceAsync(string, Message{TKey, TValue}, CancellationToken)"/>
        /// method is used.
        /// </param>
        /// <param name="config">
        /// A collection of librdkafka configuration parameters (refer to
        /// https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md) and parameters
        /// specific to this client (refer to: Confluent.Kafka.ConfigPropertyNames).
        /// </param>
        public KafkaSender(string name, string topic, string bootstrapServers, bool useBeginProduce = true, ProducerConfig config = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            UseBeginProduce = useBeginProduce;
            Config = config ?? new ProducerConfig();
            Config.BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));

            var producerBuilder = new ProducerBuilder<Null, string>(Config);
            _producer = new Lazy<Producer<Null, string>>(() => producerBuilder.Build());
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
        /// Gets a value indicating which method is used to send messages.
        /// </summary>
        public bool UseBeginProduce { get; }

        /// <summary>
        /// Gets the configuration that is used to create the <see cref="Producer{TKey, TValue}"/> for this receiver.
        /// </summary>
        public ProducerConfig Config { get; }

        /// <summary>
        /// Flushes the produces and disposes it.
        /// </summary>
        public void Dispose()
        {
            if (_producer.IsValueCreated)
            {
                _producer.Value.Flush(TimeSpan.FromSeconds(10));
                _producer.Value.Dispose();
            }
        }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "Kafka";

            var kafkaMessage = new Message<Null, string> { Value = message.StringPayload };

            if (message.Headers.Count > 0)
            {
                kafkaMessage.Headers = kafkaMessage.Headers ?? new Headers();
                foreach (var header in message.Headers)
                    kafkaMessage.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value.ToString()));
            }

            if (UseBeginProduce)
            {
                try
                {
                    _producer.Value.BeginProduce(Topic, kafkaMessage);
                }
                catch (Exception ex)
                {
                    return Tasks.FromException(ex);
                }
                
                return Tasks.CompletedTask;
            }

            return _producer.Value.ProduceAsync(Topic, kafkaMessage, cancellationToken);
        }
    }
}

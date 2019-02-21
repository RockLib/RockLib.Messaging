using Confluent.Kafka;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    public class KafkaSender : ISender
    {
        private readonly Lazy<Producer<Null, string>> _producer;

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

        public string Name { get; }
        public string Topic { get; }
        public bool UseBeginProduce { get; }
        public ProducerConfig Config { get; }

        public void Dispose()
        {
            if (_producer.IsValueCreated)
            {
                _producer.Value.Flush(TimeSpan.FromSeconds(10));
                _producer.Value.Dispose();
            }
        }

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

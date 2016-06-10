using System;
using Rock.Immutable;
using Rock.Serialization;

namespace Rock.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="IMessagingScenarioFactory"/> that returns
    /// instances of <see cref="ISender"/> and <see cref="IReceiver"/> that use
    /// named pipes as their communication mechanism.
    /// </summary>
    public class NamedPipeMessagingScenarioFactory : IMessagingScenarioFactory
    {
        private static readonly Semimutable<INamedPipeConfigProvider> _defaultConfigProvider =
            new Semimutable<INamedPipeConfigProvider>(CreateDefaultDefaultConfigProvider);

        private readonly INamedPipeConfigProvider _configProvider;
        private readonly ISerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeMessagingScenarioFactory"/> class.
        /// </summary>
        public NamedPipeMessagingScenarioFactory()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeMessagingScenarioFactory"/> class.
        /// </summary>
        /// <param name="configProvider">
        /// The configuration provider. If null or not provided, then
        /// <see cref="DefaultConfigProvider"/> is used.
        /// </param>
        /// <param name="serializer">
        /// The serializer to use when sending or receiving messages. If null or not provided,
        /// then a JSON serializer is used.
        /// </param>
        public NamedPipeMessagingScenarioFactory(
            INamedPipeConfigProvider configProvider = null,
            ISerializer serializer = null)
        {
            _configProvider = configProvider ?? DefaultConfigProvider;
            _serializer = serializer ?? new SenderMessageJsonSerializer();
        }

        public static INamedPipeConfigProvider DefaultConfigProvider
        {
            get { return _defaultConfigProvider.Value; }
        }

        public static void SetDefaultConfigProvider(INamedPipeConfigProvider defaultConfigProvider)
        {
            _defaultConfigProvider.Value = defaultConfigProvider;
        }

        private static INamedPipeConfigProvider CreateDefaultDefaultConfigProvider()
        {
            return new SimpleNamedPipeConfigProvider();
        }

        /// <summary>
        /// Creates an instance of <see cref="ISender" /> that uses the queue producer scenario.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>
        /// An instance of <see cref="ISender" /> that uses the queue producer scenario.
        /// </returns>
        public ISender CreateQueueProducer(string name)
        {
            var config = _configProvider.GetConfig(name);
            return new NamedPipeQueueProducer(name, config.PipeName, config.Compressed, _serializer);
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver" /> that uses the queue consumer scenario.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>
        /// An instance of <see cref="IReceiver" /> that uses the queue consumer scenario.
        /// </returns>
        public IReceiver CreateQueueConsumer(string name)
        {
            var config = _configProvider.GetConfig(name);
            return new NamedPipeQueueConsumer(name, config.PipeName, _serializer);
        }

        /// <summary>
        /// Creates an instance of <see cref="ISender" /> that uses the topic publisher scenario.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>
        /// An instance of <see cref="ISender" /> that uses the topic publisher scenario.
        /// </returns>
        public ISender CreateTopicPublisher(string name)
        {
            return CreateQueueProducer(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver" /> that uses the topic subscriber scenario.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>
        /// An instance of <see cref="IReceiver" /> that uses the topic subscriber scenario.
        /// </returns>
        public IReceiver CreateTopicSubscriber(string name)
        {
            return CreateQueueConsumer(name);
        }

        public bool HasScenario(string name)
        {
            return _configProvider.HasConfig(name);
        }

        void IDisposable.Dispose()
        {
        }
    }
}

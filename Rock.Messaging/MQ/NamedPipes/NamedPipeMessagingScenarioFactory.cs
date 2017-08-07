using System;
using RockLib.Messaging.MQ.NamedPipes;
#if ROCKLIB
using RockLib.Immutable;
#else
using Rock.Immutable;
#endif

#if ROCKLIB
namespace RockLib.Messaging.NamedPipes
#else
namespace Rock.Messaging.NamedPipes
#endif
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeMessagingScenarioFactory"/> class.
        /// </summary>
        public NamedPipeMessagingScenarioFactory()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeMessagingScenarioFactory"/> class.
        /// </summary>
        /// <param name="configProvider">
        /// The configuration provider. If null or not provided, then
        /// <see cref="DefaultConfigProvider"/> is used.
        /// </param>
        public NamedPipeMessagingScenarioFactory(INamedPipeConfigProvider configProvider = null)
        {
            _configProvider = configProvider ?? DefaultConfigProvider;
        }

#if ROCKLIB
        public MessagingSettings[] MessagingSettings { get; set; }
#endif

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
            return new NamedPipeQueueProducer(name, config.PipeName, config.Compressed);
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
//            var config = _configProvider.GetConfig(name);
//            return new NamedPipeQueueConsumer(name, config.PipeName);

            throw new NotImplementedException("Need to get working ");
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

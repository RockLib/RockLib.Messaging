using System;
using System.Collections.Generic;

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
        private readonly INamedPipeConfigProvider _configProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeMessagingScenarioFactory"/> class.
        /// </summary>
        public NamedPipeMessagingScenarioFactory(List<NamedPipeConfig> namedPipeConfigs)
            : this(new NamedPipeConfigProvider(namedPipeConfigs))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeMessagingScenarioFactory"/> class.
        /// </summary>
        /// <param name="configProvider">
        /// The configuration provider. If null or not provided, then
        /// a <see cref="NamedPipeConfigProvider"/> is used.
        /// </param>
        public NamedPipeMessagingScenarioFactory(INamedPipeConfigProvider configProvider = null)
        {
            _configProvider = configProvider ?? new NamedPipeConfigProvider();
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
            var config = _configProvider.GetConfig(name);
            return new NamedPipeQueueConsumer(name, config.PipeName);
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

        /// <summary>
        /// Returns a value indicating whether a scenario by the given name can be created by this
        /// instance of <see cref="NamedPipeMessagingScenarioFactory"/>.
        /// </summary>s
        /// <param name="name">The name of the scenario.</param>
        /// <returns>True, if the scenario can be created. Otherwise, false.</returns>
        public bool HasScenario(string name)
        {
            return _configProvider.HasConfig(name);
        }

        void IDisposable.Dispose()
        {
        }
    }
}

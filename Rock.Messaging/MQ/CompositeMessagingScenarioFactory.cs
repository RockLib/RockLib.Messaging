using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Messaging;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    /// <summary>
    /// An implementation of <see cref="IMessagingScenarioFactory"/> that delegates behaviour to one
    /// of many instance of <see cref="IMessagingScenarioFactory"/>.
    /// </summary>
    public class CompositeMessagingScenarioFactory : IMessagingScenarioFactory
    {
        private readonly IEnumerable<IMessagingScenarioFactory> _factories;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeMessagingScenarioFactory"/> class.
        /// </summary>
        /// <param name="factories">A collection of <see cref="IMessagingScenarioFactory"/> instances.</param>
        public CompositeMessagingScenarioFactory(IEnumerable<IMessagingScenarioFactory> factories)
        {
            if (factories == null) throw new ArgumentNullException("factories");
            _factories = factories;
        }

        /// <summary>
        /// Gets the collection of <see cref="IMessagingScenarioFactory"/> instances that this
        /// instance of <see cref="CompositeMessagingScenarioFactory"/> uses internally.
        /// </summary>
        public IEnumerable<IMessagingScenarioFactory> Factories
        {
            get { return _factories; }
        }

        ISender IMessagingScenarioFactory.CreateQueueProducer(string name)
        {
            return GetFactory(name).CreateQueueProducer(name);
        }

        IReceiver IMessagingScenarioFactory.CreateQueueConsumer(string name)
        {
            return GetFactory(name).CreateQueueConsumer(name);
        }

        ISender IMessagingScenarioFactory.CreateTopicPublisher(string name)
        {
            return GetFactory(name).CreateTopicPublisher(name);
        }

        IReceiver IMessagingScenarioFactory.CreateTopicSubscriber(string name)
        {
            return GetFactory(name).CreateTopicSubscriber(name);
        }

        bool IMessagingScenarioFactory.HasScenario(string name)
        {
            return Factories.Any(f => f.HasScenario(name));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var factory in Factories)
            {
                factory.Dispose();
            }
        }

        private IMessagingScenarioFactory GetFactory(string name)
        {
            try
            {
                return Factories.First(f => f.HasScenario(name));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to locate a messaging scenario with the name '{name}'.", ex);
            }
        }
    }
}
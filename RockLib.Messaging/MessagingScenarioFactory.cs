using System;
using System.Collections.Generic;
using System.Linq;
using RockLib.Configuration;
using RockLib.Immutable;
using RockLib.Configuration.ObjectFactory;

namespace RockLib.Messaging
{
    /// <summary>
    /// Provides methods for creating instances of various messaging scenarios.
    /// </summary>
    public static class MessagingScenarioFactory
    {
        private static readonly Semimutable<IMessagingScenarioFactory> _messagingScenarioFactory =
            new Semimutable<IMessagingScenarioFactory>(CreateDefaultMessagingScenarioFactory);

        private static IMessagingScenarioFactory _fallbackMessagingScenarioFactory;

        /// <summary>
        /// Gets the current instance of <see cref="IMessagingScenarioFactory"/> used for operations
        /// of the <see cref="MessagingScenarioFactory"/> class.
        /// </summary>
        public static IMessagingScenarioFactory Current => _messagingScenarioFactory.Value;

        /// <summary>
        /// Sets the current instance of <see cref="IMessagingScenarioFactory"/> used for operations
        /// of the <see cref="MessagingScenarioFactory"/> class.
        /// </summary>
        /// <param name="messagingScenarioFactory">
        /// The instance of <see cref="IMessagingScenarioFactory"/> to be used for operations of the
        /// <see cref="MessagingScenarioFactory"/> class.
        /// </param>
        public static void SetCurrent(IMessagingScenarioFactory messagingScenarioFactory)
        {
            _messagingScenarioFactory.SetValue(() =>
            {
                _fallbackMessagingScenarioFactory = null;
                return messagingScenarioFactory;
            });
        }

        internal static void SetFallback(IMessagingScenarioFactory messagingScenarioFactory)
        {
            _fallbackMessagingScenarioFactory = messagingScenarioFactory;
        }

        private static IMessagingScenarioFactory CreateDefaultMessagingScenarioFactory()
        {
            try
            {
                IMessagingScenarioFactory value;

                return TryGetFactoryFromConfig(out value) ? value : _fallbackMessagingScenarioFactory ?? ThrowNoMessagingScenarioFactoryFoundException();
            }
            finally
            {
                _fallbackMessagingScenarioFactory = null;
            }
        }

        private static IMessagingScenarioFactory ThrowNoMessagingScenarioFactoryFoundException()
        {
            throw new InvalidOperationException("MessagingScenarioFactory.Current has no value. The value can be set via config or by calling the SetCurrent method.");
        }

        private static bool TryGetFactoryFromConfig(out IMessagingScenarioFactory factory)
        {
            try
            {

                factory = BuildFactory();

                return true;
            }
            catch (Exception)
            {
                factory = null;
                return false;
            }
        }

        internal static IMessagingScenarioFactory BuildFactory()
        {
            var messagingSection = Config.Root.GetSection("RockLib.Messaging");
            var scenarioFactories = messagingSection.Create<List<IMessagingScenarioFactory>>();

            if( !scenarioFactories.Any()) { throw new InvalidOperationException("There must be at least one scenario factory, please make sure your configuration is correct."); }

            return scenarioFactories.Count == 1 ? scenarioFactories[0] : new CompositeMessagingScenarioFactory(scenarioFactories);
        }
        

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the queue producer scenario
        /// by calling the <see cref="IMessagingScenarioFactory.CreateQueueProducer"/> method
        /// on <see cref="Current"/>.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the queue producer scenario.</returns>
        public static ISender CreateQueueProducer(string name)
        {
            return Current.CreateQueueProducer(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the queue consumer scenario
        /// by calling the <see cref="IMessagingScenarioFactory.CreateQueueConsumer"/> method
        /// on <see cref="Current"/>.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the queue consumer scenario.</returns>
        public static IReceiver CreateQueueConsumer(string name)
        {
            return Current.CreateQueueConsumer(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the topic publisher scenario
        /// by calling the <see cref="IMessagingScenarioFactory.CreateTopicPublisher"/> method
        /// on <see cref="Current"/>.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the topic publisher scenario.</returns>
        public static ISender CreateTopicPublisher(string name)
        {
            return Current.CreateTopicPublisher(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the topic subscriber scenario
        /// by calling the <see cref="IMessagingScenarioFactory.CreateTopicSubscriber"/> method
        /// on <see cref="Current"/>.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the topic subscriber scenario.</returns>
        public static IReceiver CreateTopicSubscriber(string name)
        {
            return Current.CreateTopicSubscriber(name);
        }
    }
}
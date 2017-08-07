using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using RockLib.Configuration;
using RockLib.Messaging.Configuration;
#if ROCKLIB
using RockLib.Immutable;
#else
using Rock.Immutable;
#endif


#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    /// <summary>
    /// Provides methods for creating instances of various messaging scenarios.
    /// </summary>
    public static class MessagingScenarioFactory
    {
        private static readonly Semimutable<IMessagingScenarioFactory> _messagingScenarioFactory =
            new Semimutable<IMessagingScenarioFactory>(CreateDefaultMessagingScenarioFactory);

        private static IMessagingScenarioFactory _fallbackMessagingScenarioFactory;

        public static IMessagingScenarioFactory Current
        {
            get { return _messagingScenarioFactory.Value; }
        }

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

                return
                    TryGetFactoryFromConfig(out value)
                        ? value
                        : _fallbackMessagingScenarioFactory ?? ThrowNoMessagingScenarioFactoryFoundException();
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

#if NETSTANDARD1_6
                factory = BuildFactoryForCore();
#else
                factory = BuildConfigurationFromFramework();
#endif

                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError($"[Rock.Messaging] - [Messaging Scenario Factory] - TryGet Config failed with {e.Message}");

                factory = null;
                return false;
            }
        }

        internal static IMessagingScenarioFactory BuildFactoryForCore()
        {
            var messagingSection = Config.Root.GetSection("RockLib.Messaging").Get<ScenarioFactorySection>();
            var scenarioFactories = messagingSection.ScenarioFactories;

            if( !scenarioFactories.Any()) { throw new InvalidOperationException("FactoryProxies must have at least one element, please make sure your configuration is correct."); }

            var factories = scenarioFactories.Select(f => f.CreateInstance()).ToList();

            return factories.Count == 1 ? factories[0] : new CompositeMessagingScenarioFactory(factories);
        }

        private static IMessagingScenarioFactory BuildConfigurationFromFramework()
        {
            //(IRockMessagingConfiguration)ConfigurationManager.GetSection("rock.messaging");
            throw new NotImplementedException();
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
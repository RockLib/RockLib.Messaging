using System;
using System.Configuration;

namespace Rock.Messaging
{
    /// <summary>
    /// Provides methods for creating instances of various messaging scenarios.
    /// </summary>
    public static class MessagingScenarioFactory
    {
        private static Lazy<IMessagingScenarioFactory> _current = new Lazy<IMessagingScenarioFactory>(GetFactory);

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the queue producer scenario
        /// by calling the <see cref="MessagingScenarioFactoryExtensions.CreateQueueProducer"/> extension method
        /// on the <see cref="Current"/> property.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the queue producer scenario.</returns>
        public static ISender CreateQueueProducer(string name)
        {
            return Current.CreateQueueProducer(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the queue consumer scenario
        /// by calling the <see cref="MessagingScenarioFactoryExtensions.CreateQueueConsumer"/> extension method
        /// on the <see cref="Current"/> property.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the queue consumer scenario.</returns>
        public static IReceiver CreateQueueConsumer(string name)
        {
            return Current.CreateQueueConsumer(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the topic publisher scenario
        /// by calling the <see cref="MessagingScenarioFactoryExtensions.CreateTopicPublisher"/> extension method
        /// on the <see cref="Current"/> property.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the topic publisher scenario.</returns>
        public static ISender CreateTopicPublisher(string name)
        {
            return Current.CreateTopicPublisher(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the topic subscriber scenario
        /// by calling the <see cref="MessagingScenarioFactoryExtensions.CreateTopicSubscriber"/> extension method
        /// on the <see cref="Current"/> property.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the topic subscriber scenario.</returns>
        public static IReceiver CreateTopicSubscriber(string name)
        {
            return Current.CreateTopicSubscriber(name);
        }

        /// <summary>
        /// Gets or sets the <see cref="IMessagingScenarioFactory"/> object that is used
        /// by the various methods of <see cref="MessagingScenarioFactory"/>. If set to
        /// null, the value will revert to the default value.
        /// </summary>
        public static IMessagingScenarioFactory Current
        {
            get { return _current.Value; }
            set
            {
                Func<IMessagingScenarioFactory> createFactory;

                if (value == null)
                {
                    createFactory = GetFactory;
                }
                else
                {
                    createFactory = () => value;
                }

                _current = new Lazy<IMessagingScenarioFactory>(createFactory);
            }
        }

        private static IMessagingScenarioFactory GetFactory()
        {
            IMessagingScenarioFactory value;

            return
                TryGetFactoryFromConfig(out value)
                    ? value
                    : GetDefaultFactory();
        }

        private static bool TryGetFactoryFromConfig(out IMessagingScenarioFactory factory)
        {
            //try
            //{
            //    var rockFrameworkSection = (RockFrameworkSection)ConfigurationManager.GetSection("rock.framework");
            //    var messagingScenarioFactory = rockFrameworkSection.MessagingSettings.MessagingScenarioFactory;
            //    factory = (IMessagingScenarioFactory)Activator.CreateInstance(Type.GetType(messagingScenarioFactory.Type));
            //    return true;
            //}
            //catch (Exception)
            //{
                factory = null;
                return false;
            //}
        }

        private static IMessagingScenarioFactory GetDefaultFactory()
        {
            //return new SonicMessagingScenarioFactory(new FileSonicConfigProvider());
            return null;
        }
    }
}
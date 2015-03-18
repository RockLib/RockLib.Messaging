using System.Linq;

namespace Rock.Messaging
{
    /// <summary>
    /// Provides a set of static methods for creating various messaging scenarios using an instance of <see cref="IMessagingScenarioFactory"/>.
    /// </summary>
    public static class MessagingScenarioFactoryExtensions
    {
        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the queue producer scenario.
        /// </summary>
        /// <param name="source">The factory from which to create the scenario.</param>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the queue producer scenario.</returns>
        public static ISender CreateQueueProducer(this IMessagingScenarioFactory source, string name)
        {
            return source.CreateQueueProducers(name, 1).First();
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the queue consumer scenario.
        /// </summary>
        /// <param name="source">The factory from which to create the scenario.</param>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the queue consumer scenario.</returns>
        public static IReceiver CreateQueueConsumer(this IMessagingScenarioFactory source, string name)
        {
            return source.CreateQueueConsumers(name, 1).First();
        }

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the topic publisher scenario.
        /// </summary>
        /// <param name="source">The factory from which to create the scenario.</param>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the topic publisher scenario.</returns>
        public static ISender CreateTopicPublisher(this IMessagingScenarioFactory source, string name)
        {
            return source.CreateTopicPublishers(name, 1).First();
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the topic subscriber scenario.
        /// </summary>
        /// <param name="source">The factory from which to create the scenario.</param>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the topic subscriber scenario.</returns>
        public static IReceiver CreateTopicSubscriber(this IMessagingScenarioFactory source, string name)
        {
            return source.CreateTopicSubscribers(name, 1).First();
        }
    }
}
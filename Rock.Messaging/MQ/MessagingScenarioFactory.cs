using Rock.Messaging.Defaults.Implementation;

namespace Rock.Messaging
{
    /// <summary>
    /// Provides methods for creating instances of various messaging scenarios.
    /// </summary>
    public static class MessagingScenarioFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the queue producer scenario
        /// by calling the <see cref="IMessagingScenarioFactory.CreateQueueProducer"/> method
        /// on <see cref="Default.MessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the queue producer scenario.</returns>
        public static ISender CreateQueueProducer(string name)
        {
            return Default.MessagingScenarioFactory.CreateQueueProducer(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the queue consumer scenario
        /// by calling the <see cref="IMessagingScenarioFactory.CreateQueueConsumer"/> method
        /// on <see cref="Default.MessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the queue consumer scenario.</returns>
        public static IReceiver CreateQueueConsumer(string name)
        {
            return Default.MessagingScenarioFactory.CreateQueueConsumer(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the topic publisher scenario
        /// by calling the <see cref="IMessagingScenarioFactory.CreateTopicPublisher"/> method
        /// on <see cref="Default.MessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the topic publisher scenario.</returns>
        public static ISender CreateTopicPublisher(string name)
        {
            return Default.MessagingScenarioFactory.CreateTopicPublisher(name);
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the topic subscriber scenario
        /// by calling the <see cref="IMessagingScenarioFactory.CreateTopicSubscriber"/> method
        /// on <see cref="Default.MessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the topic subscriber scenario.</returns>
        public static IReceiver CreateTopicSubscriber(string name)
        {
            return Default.MessagingScenarioFactory.CreateTopicSubscriber(name);
        }
    }
}
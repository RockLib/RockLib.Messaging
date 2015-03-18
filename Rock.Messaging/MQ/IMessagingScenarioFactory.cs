using System.Collections.Generic;

namespace Rock.Messaging
{
    /// <summary>
    /// Defines an interface for creating instances of various messaging scenarios.
    /// </summary>
    public interface IMessagingScenarioFactory
    {
        /// <summary>
        /// Creates instances of <see cref="ISender"/> that use the queue producer scenario.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <param name="count">The number of instances of <see cref="ISender"/> to return.</param>
        /// <returns>Instances of <see cref="ISender"/> that use the queue producer scenario.</returns>
        IEnumerable<ISender> CreateQueueProducers(string name, int count);

        /// <summary>
        /// Creates instances of <see cref="IReceiver"/> that use the queue consumer scenario.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <param name="count">The number of instances of <see cref="IReceiver"/> to return.</param>
        /// <returns>Instances of <see cref="IReceiver"/> that use the queue consumer scenario.</returns>
        IEnumerable<IReceiver> CreateQueueConsumers(string name, int count);

        /// <summary>
        /// Creates instances of <see cref="ISender"/> that use the topic publisher scenario.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <param name="count">The number of instances of <see cref="ISender"/> to return.</param>
        /// <returns>Instances of <see cref="ISender"/> that use the topic publisher scenario.</returns>
        IEnumerable<ISender> CreateTopicPublishers(string name, int count);

        /// <summary>
        /// Creates instances of <see cref="IReceiver"/> that use the topic subscriber scenario.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <param name="count">The number of instances of <see cref="IReceiver"/> to return.</param>
        /// <returns>Instances of <see cref="IReceiver"/> that use the topic subscriber scenario.</returns>
        IEnumerable<IReceiver> CreateTopicSubscribers(string name, int count);
    }
}
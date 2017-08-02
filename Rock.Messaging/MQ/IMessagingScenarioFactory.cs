using System;
using Rock.Messaging;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    /// <summary>
    /// Defines an interface for creating instances of various messaging scenarios.
    /// </summary>
    public interface IMessagingScenarioFactory : IDisposable
    {
        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the queue producer scenario.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the queue producer scenario.</returns>
        /// <remarks>
        /// Note to implementors of this interface: If this scenario is not applicable,
        /// return the result of a call to <see cref="CreateTopicPublisher"/> instead of
        /// throwing an exception.
        /// </remarks>
        ISender CreateQueueProducer(string name);

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the queue consumer scenario.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the queue consumer scenario.</returns>
        /// <remarks>
        /// Note to implementors of this interface: If this scenario is not applicable,
        /// return the result of a call to <see cref="CreateTopicSubscriber"/> instead of
        /// throwing an exception.
        /// </remarks>
        IReceiver CreateQueueConsumer(string name);

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the topic publisher scenario.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the topic publisher scenario.</returns>
        /// <remarks>
        /// Note to implementors of this interface: If this scenario is not applicable,
        /// return the result of a call to <see cref="CreateQueueProducer"/> instead of
        /// throwing an exception.
        /// </remarks>
        ISender CreateTopicPublisher(string name);

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the topic subscriber scenario.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the topic subscriber scenario.</returns>
        /// <remarks>
        /// Note to implementors of this interface: If this scenario is not applicable,
        /// return the result of a call to <see cref="CreateQueueConsumer"/> instead of
        /// throwing an exception.
        /// </remarks>
        IReceiver CreateTopicSubscriber(string name);

        /// <summary>
        /// Returns a value indicating whether a scenario by the given name can be created by this
        /// instance of <see cref="IMessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="name">The name of the scenario.</param>
        /// <returns>True, if the scenario can be created. Otherwise, false.</returns>
        bool HasScenario(string name);
    }
}
namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// Contains various settings required by <see cref="SQSQueueReceiver"/>
    /// and <see cref="SQSQueueSender"/>.
    /// </summary>
    public interface ISQSConfiguration
    {

        /// <summary>
        /// Gets the configuration name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the url of the SQS queue.
        /// </summary>
        string QueueUrl { get; }

        /// <summary>
        /// Gets the maximum number of messages to return with each call to the SQS endpoint.
        /// Amazon SQS never returns more messages than this value (however, fewer messages
        /// might be returned). Valid values are 1 to 10.
        /// </summary>
        int MaxMessages { get; }

        /// <summary>
        /// Gets a value indicating whether messages will be automatically acknowledged after
        /// anyevent handler execute.
        /// </summary>
        bool AutoAcknowledge { get; }

        /// <summary>
        /// Gets a value indicating whether message payloads should be compressed before
        /// sending.
        /// </summary>
        bool Compressed { get; }

        /// <summary>
        /// Gets a value indicating whether, in the case of when multiple messages are
        /// received from an SQS request, messages are handled in parallel or sequentially.
        /// </summary>
        bool ParallelHandling { get; }
    }
}
namespace RockLib.Messaging
{
    /// <summary>
    /// Defines the interface for a received message.
    /// </summary>
    public interface IReceiverMessage
    {
        /// <summary>
        /// Gets the payload of the message as a string.
        /// </summary>
        string StringPayload { get; }

        /// <summary>
        /// Gets the payload of the message as a byte array.
        /// </summary>
        byte[] BinaryPayload { get; }

        /// <summary>
        /// Gets the headers of the message.
        /// </summary>
        HeaderDictionary Headers { get; }

        /// <summary>
        /// Gets the priority of the message, if applicable.
        /// </summary>
        byte? Priority { get; }

        /// <summary>
        /// If supported by the implementation, communicate to the sender that
        /// the message was successfully processed and should not be redelivered.
        /// </summary>
        void Acknowledge();

        /// <summary>
        /// If supported by the implementation, communicate to the sender that
        /// the message was not successfully processed and should be redelivered.
        /// </summary>
        void Rollback();

        /// <summary>
        /// If supported by the implementation, communicate to the sender that
        /// the message was not successfully processed and should not be redelivered.
        /// </summary>
        void Reject();
    }
}
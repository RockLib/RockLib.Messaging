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
        /// Gets a value indicating whether the message is transactional. If true,
        /// either the <see cref="Acknowledge"/> or <see cref="Rollback"/> method must
        /// be called when processing the message.
        /// </summary>
        bool IsTransactional { get; }

        /// <summary>
        /// If <see cref="IsTransactional"/> is true, communicate to the server that
        /// the message was successfully processed and should not be redelivered.
        /// </summary>
        void Acknowledge();

        /// <summary>
        /// If <see cref="IsTransactional"/> is true, communicate to the server that
        /// the message was not successfully processed and should be redelivered.
        /// </summary>
        void Rollback();
    }
}
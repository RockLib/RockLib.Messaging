namespace Rock.Messaging
{
    /// <summary>
    /// Defines an interface for a messaging transaction.
    /// </summary>
    public interface ISenderTransaction
    {
        /// <summary>
        /// Adds the specified message to the transaction.
        /// </summary>
        /// <param name="message">The message to add.</param>
        void Add(ISenderMessage message);

        /// <summary>
        /// Commits any messages that were added to the transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back any messages that were added to the transaction.
        /// </summary>
        void Rollback();
    }
}
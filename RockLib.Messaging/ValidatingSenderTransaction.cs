using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// A decorator for the <see cref="ISenderTransaction"/> interface that invokes a <c>validate</c> delegate
    /// before each message is added to the transaction.
    /// </summary>
    public class ValidatingSenderTransaction : ISenderTransaction
    {
        internal ValidatingSenderTransaction(ISenderTransaction transaction, Action<SenderMessage> validate)
        {
            Transaction = transaction;
            Validate = validate;
        }

        /// <summary>
        /// Gets the actual <see cref="ISenderTransaction"/>.
        /// </summary>
        public ISenderTransaction Transaction { get; }

        /// <summary>
        /// Gets the delegate that will be invoked before each message added to a transaction.
        /// </summary>
        public Action<SenderMessage> Validate { get; }

        /// <summary>
        /// Validates, then adds the specified message to the transaction.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public void Add(SenderMessage message)
        {
            Validate(message);
            Transaction.Add(message);
        }

        /// <summary>
        /// Commits any messages that were added to the transaction.
        /// </summary>
        public void Commit() => Transaction.Commit();

        /// <summary>
        /// Rolls back any messages that were added to the transaction.
        /// </summary>
        public void Rollback() => Transaction.Rollback();
    }
}

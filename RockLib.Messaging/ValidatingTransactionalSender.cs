using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// A decorator for the <see cref="ITransactionalSender"/> interface that invokes a <c>validate</c> delegate
    /// before each message is sent or added to a transaction.
    /// </summary>
    public class ValidatingTransactionalSender : ValidatingSender, ITransactionalSender
    {
        /// <summary>
        /// Initializes a new instances of the <see cref="ValidatingTransactionalSender"/> class.
        /// </summary>
        /// <param name="name">The name of the validating sender.</param>
        /// <param name="transactionalSender">
        /// The <see cref="ITransactionalSender"/> that actually sends messages and begins transactions.
        /// </param>
        /// <param name="validate">
        /// A delegate that will be invoked before each message is sent or added to a transaction. The delegate
        /// should throw an exception if header values are invalid or if required headers are missing. The delegate
        /// may also add missing headers that can be calculated at runtime.
        /// </param>
        public ValidatingTransactionalSender(string name, ITransactionalSender transactionalSender, Action<SenderMessage> validate)
            : base(name, transactionalSender, validate)
        {
            TransactionalSender = transactionalSender;
        }

        /// <summary>
        /// Gets the <see cref="ITransactionalSender"/> that actually sends messages and begins transactions.
        /// </summary>
        public ITransactionalSender TransactionalSender { get; }

        /// <summary>
        /// Starts a message-sending transaction that validates messages as they are added.
        /// </summary>
        /// <returns>An object representing the new validating transaction.</returns>
        public ISenderTransaction BeginTransaction() =>
            new ValidatingSenderTransaction(TransactionalSender.BeginTransaction(), Validate);
    }
}

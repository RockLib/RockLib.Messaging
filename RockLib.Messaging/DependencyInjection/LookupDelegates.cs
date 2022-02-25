namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Represents a method that retrieves an <see cref="ISender"/> by its name.
    /// </summary>
    /// <param name="name">The name of the <see cref="ISender"/> to retrieve.</param>
    /// <returns>The matching <see cref="ISender"/>.</returns>
    public delegate ISender SenderLookup(string name);

    /// <summary>
    /// Represents a method that retrieves an <see cref="ITransactionalSender"/> by its name.
    /// </summary>
    /// <param name="name">The name of the <see cref="ITransactionalSender"/> to retrieve.</param>
    /// <returns>The matching <see cref="ITransactionalSender"/>.</returns>
    public delegate ITransactionalSender TransactionalSenderLookup(string name);

    /// <summary>
    /// Represents a method that retrieves an <see cref="IReceiver"/> by its name.
    /// </summary>
    /// <param name="name">The name of the <see cref="IReceiver"/> to retrieve.</param>
    /// <returns>The matching <see cref="IReceiver"/>.</returns>
    public delegate IReceiver ReceiverLookup(string name);
}
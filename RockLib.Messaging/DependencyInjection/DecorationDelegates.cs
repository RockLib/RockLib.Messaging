using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Represents a method that creates an <see cref="ISender"/> decorator.
    /// </summary>
    /// <param name="sender">The <see cref="ISender"/> that is being decorated.</param>
    /// <param name="serviceProvider">
    /// The <see cref="IServiceProvider"/> that resolves dependencies necessary for the creation of the
    /// <see cref="ISender"/> decorator.
    /// </param>
    /// <returns>An <see cref="ISender"/> decorator.</returns>
    public delegate ISender SenderDecoration(ISender sender, IServiceProvider serviceProvider);

    /// <summary>
    /// Represents a method that creates an <see cref="ITransactionalSender"/> decorator.
    /// </summary>
    /// <param name="sender">The <see cref="ITransactionalSender"/> that is being decorated.</param>
    /// <param name="serviceProvider">
    /// The <see cref="IServiceProvider"/> that resolves dependencies necessary for the creation of the
    /// <see cref="ITransactionalSender"/> decorator.
    /// </param>
    /// <returns>An <see cref="ITransactionalSender"/> decorator.</returns>
    public delegate ITransactionalSender TransactionalSenderDecoration(ITransactionalSender sender, IServiceProvider serviceProvider);

    /// <summary>
    /// Represents a method that creates an <see cref="IReceiver"/> decorator.
    /// </summary>
    /// <param name="receiver">The <see cref="IReceiver"/> that is being decorated.</param>
    /// <param name="serviceProvider">
    /// The <see cref="IServiceProvider"/> that resolves dependencies necessary for the creation of the
    /// <see cref="IReceiver"/> decorator.
    /// </param>
    /// <returns>An <see cref="IReceiver"/> decorator.</returns>
    public delegate IReceiver ReceiverDecoration(IReceiver receiver, IServiceProvider serviceProvider);
}
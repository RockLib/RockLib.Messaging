using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Represents a method that creates an <see cref="ISender"/>.
    /// </summary>
    /// <param name="serviceProvider">
    /// The <see cref="IServiceProvider"/> that resolves dependencies necessary for the creation of the
    /// <see cref="ISender"/>.
    /// </param>
    /// <returns>An <see cref="ISender"/>.</returns>
    public delegate ISender SenderRegistration(IServiceProvider serviceProvider);

    /// <summary>
    /// Represents a method that creates an <see cref="ITransactionalSender"/>.
    /// </summary>
    /// <param name="serviceProvider">
    /// The <see cref="IServiceProvider"/> that resolves dependencies necessary for the creation of the
    /// <see cref="ITransactionalSender"/>.
    /// </param>
    /// <returns>An <see cref="ITransactionalSender"/>.</returns>
    public delegate ITransactionalSender TransactionalSenderRegistration(IServiceProvider serviceProvider);

    /// <summary>
    /// Represents a method that creates an <see cref="IReceiver"/>.
    /// </summary>
    /// <param name="serviceProvider">
    /// The <see cref="IServiceProvider"/> that resolves dependencies necessary for the creation of the
    /// <see cref="IReceiver"/>.
    /// </param>
    /// <returns>An <see cref="IReceiver"/>.</returns>
    public delegate IReceiver ReceiverRegistration(IServiceProvider serviceProvider);
}

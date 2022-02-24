namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// A builder used to decorate an <see cref="ITransactionalSender"/>.
    /// </summary>
    public interface ITransactionalSenderBuilder
    {
        /// <summary>
        /// Adds a decoration delegate to the builder.
        /// </summary>
        /// <param name="decoration">The decoration delegate.</param>
        ITransactionalSenderBuilder AddDecorator(TransactionalSenderDecoration decoration);
    }
}
#if !NET451
namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// A builder used to decorate an <see cref="IReceiver"/>.
    /// </summary>
    public interface IReceiverBuilder
    {
        /// <summary>
        /// Adds a decoration delegate to the builder.
        /// </summary>
        /// <param name="decoration">The decoration delegate.</param>
        IReceiverBuilder AddDecorator(ReceiverDecoration decoration);
    }
}
#endif

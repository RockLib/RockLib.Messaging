#if !NET451
namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// A builder used to decorate an <see cref="ISender"/>.
    /// </summary>
    public interface ISenderBuilder
    {
        /// <summary>
        /// Adds a decoration delegate to the builder.
        /// </summary>
        /// <param name="decoration">The decoration delegate.</param>
        ISenderBuilder AddDecorator(SenderDecoration decoration);
    }
}
#endif

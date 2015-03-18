using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    /// <summary>
    /// An base class for implementations of <see cref="IMessageHandler{TMessage}"/>
    /// where no return value should be returned.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    public abstract class MessageHandlerBase<TMessage> : IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        public Task<object> Handle(TMessage message)
        {
            HandleMessage(message);
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// When implemented in a derived class, handles the message. Inheritors of this
        /// method may choose to implement it asynchronously by using the 'async' modifier.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        protected abstract void HandleMessage(TMessage message);
    }
}
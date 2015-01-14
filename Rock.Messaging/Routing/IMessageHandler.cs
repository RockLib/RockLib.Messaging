using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    public interface IMessageHandler<in TMessage>
    {
        /// <summary>
        /// Handle the given message, returning a <see cref="Task{TResult}"/> whose <see cref="Task{TResult}.Result"/>
        /// property contains an object that represents the result of the operation. Note that the value of
        /// the <see cref="Task{TResult}.Result"/> property may be null.
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> whose <see cref="Task{TResult}.Result"/> property contains an
        /// <see cref="object"/> that represents the result of the operaion. Note that the value of the 
        /// <see cref="Task{TResult}.Result"/> property may be null.
        /// </returns>
        Task<object> Handle(TMessage message);
    }
}
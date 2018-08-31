using System.Threading.Tasks;

namespace RockLib.Messaging
{
    /// <summary>
    /// Provides a set of methods that simplify usage of instances of <see cref="ISender"/>
    /// and <see cref="IReceiver"/>.
    /// </summary>
    public static class MessagingScenarioExtensions
    {
        /// <summary>
        /// Synchronously sends the specified message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this ISender source, SenderMessage message)
        {
            source.SendAsync(message).Wait();
        }

        /// <summary>
        /// Synchronously sends the specified string message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this ISender source, string message)
        {
            source.SendAsync(message).Wait();
        }

        /// <summary>
        /// Asynchronously sends the specified string message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static Task SendAsync(this ISender source, string message)
        {
            return source.SendAsync(new SenderMessage(message));
        }

        /// <summary>
        /// Synchronously sends the specified binary message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this ISender source, byte[] message)
        {
            source.SendAsync(message).Wait();
        }

        /// <summary>
        /// Asynchronously sends the specified binary message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static Task SendAsync(this ISender source, byte[] message)
        {
            return source.SendAsync(new SenderMessage(message));
        }

        /// <summary>
        /// Starts listening for messages.
        /// </summary>
        /// <param name="source">The <see cref="IReceiver"/> from which to receive messages.</param>
        public static void Start(this IReceiver source)
        {
            source.Start(null);
        }
    }
}
    
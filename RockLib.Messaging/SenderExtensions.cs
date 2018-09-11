using System;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging
{
    /// <summary>
    /// Provides a set of methods that simplify usage of instances of <see cref="ISender"/>
    /// and <see cref="IReceiver"/>.
    /// </summary>
    public static class SenderExtensions
    {
        /// <summary>
        /// Synchronously sends the specified message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this ISender source, SenderMessage message) =>
            source.SendSync(s => s.SendAsync(message));

        /// <summary>
        /// Synchronously sends the specified string message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this ISender source, string message) =>
            source.SendSync(s => s.SendAsync(message));

        /// <summary>
        /// Asynchronously sends the specified string message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static Task SendAsync(this ISender source, string message) =>
            source.SendAsync(new SenderMessage(message));

        /// <summary>
        /// Synchronously sends the specified binary message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this ISender source, byte[] message) =>
            source.SendSync(s => s.SendAsync(message));

        /// <summary>
        /// Asynchronously sends the specified binary message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static Task SendAsync(this ISender source, byte[] message) =>
            source.SendAsync(new SenderMessage(message));

        private static void SendSync(this ISender source, Func<ISender, Task> sendAsync)
        {
            SynchronizationContext old = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                sendAsync(source).GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(old);
            }
        }
    }
}
    
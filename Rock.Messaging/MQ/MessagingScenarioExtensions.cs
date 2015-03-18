namespace Rock.Messaging
{
    /// <summary>
    /// Provides a set of methods that simplify usage of instances of <see cref="ISender"/>,
    /// <see cref="ISenderReceiver"/>, and <see cref="IReceiver"/>.
    /// </summary>
    public static class MessagingScenarioExtensions
    {
        /// <summary>
        /// Sends the specified string message using the <see cref="MessageFormat.Text"/> format.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this ISender source, string message)
        {
            source.Send(message, MessageFormat.Text);
        }

        /// <summary>
        /// Sends the specified string message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="messageFormat">The message's format.</param>
        public static void Send(this ISender source, string message, MessageFormat messageFormat)
        {
            source.Send(new StringSenderMessage(message, messageFormat));
        }

        /// <summary>
        /// Sends the specified binary message using the <see cref="MessageFormat.Binary"/> format.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this ISender source, byte[] message)
        {
            source.Send(message, MessageFormat.Binary);
        }

        /// <summary>
        /// Sends the specified binary message.
        /// </summary>
        /// <param name="source">The <see cref="ISender"/> from which to send the message.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="messageFormat">The message's format.</param>
        public static void Send(this ISender source, byte[] message, MessageFormat messageFormat)
        {
            source.Send(new BinarySenderMessage(message, messageFormat));
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

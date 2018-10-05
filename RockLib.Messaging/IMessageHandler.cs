namespace RockLib.Messaging
{
    /// <summary>
    /// Defines an object that handles the messages received by an instance
    /// of <see cref="IReceiver"/>.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handles a received message.
        /// </summary>
        /// <param name="receiver">The instance of <see cref="IReceiver"/> that received the message.</param>
        /// <param name="message">The message that was received.</param>
        void OnMessageReceived(IReceiver receiver, IReceiverMessage message);
    }
}
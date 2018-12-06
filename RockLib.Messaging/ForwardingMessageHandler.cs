namespace RockLib.Messaging
{
    /// <summary>
    /// A decorator for the <see cref="IMessageHandler"/> interface that forwards
    /// received messages to configured <see cref="ISender"/> instances when
    /// messages are acknowledged, rolled back, or rejected.
    /// </summary>
    public class ForwardingMessageHandler : IMessageHandler
    {
        internal ForwardingMessageHandler(ForwardingReceiver forwardingReceiver, IMessageHandler messageHandler)
        {
            ForwardingReceiver = forwardingReceiver;
            MessageHandler = messageHandler;
        }

        /// <summary>
        /// Gets the <see cref="ForwardingReceiver"/> whose properties determine
        /// what happens to messages when they are acknowledged, rolled back, or
        /// rejected. The <see cref="IReceiver"/> that this decorates is the actual
        /// source of messages.
        /// </summary>
        public ForwardingReceiver ForwardingReceiver { get; }

        /// <summary>
        /// Gets the actual <see cref="IMessageHandler"/> that gets notified when
        /// a message has been received.
        /// </summary>
        public IMessageHandler MessageHandler { get; }

        /// <summary>
        /// Handles a received message.
        /// <para>
        /// When invoked, this method invokes the <see cref="IMessageHandler.OnMessageReceived"/>
        /// method of the <see cref="MessageHandler"/> property. It passes the
        /// <see cref="ForwardingReceiver"/> property as the <c>receiver</c> argument and
        /// a new <see cref="ForwardingReceiverMessage"/> decorator as the <c>message</c>
        /// argument.
        /// </para>
        /// </summary>
        /// <param name="receiver">The instance of <see cref="IReceiver"/> that received the message.</param>
        /// <param name="message">The message that was received.</param>
        public void OnMessageReceived(IReceiver receiver, IReceiverMessage message) =>
            MessageHandler.OnMessageReceived(ForwardingReceiver, new ForwardingReceiverMessage(ForwardingReceiver, message));
    }
}

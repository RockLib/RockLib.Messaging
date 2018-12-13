using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines extension methods for the <see cref="IReceiver"/> interface.
    /// </summary>
    public static class ReceiverExtensions
    {
        /// <summary>
        /// Start listening for messages and handle them using the specified callback function.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onMessageReceived">A function that is invoked when a message is received.</param>
        public static void Start(this IReceiver receiver, Action<IReceiverMessage> onMessageReceived)
        {
            if (onMessageReceived == null) throw new ArgumentNullException(nameof(onMessageReceived));
            receiver.Start((_, message) => onMessageReceived(message));
        }

        /// <summary>
        /// Start listening for messages and handle them using the specified callback function.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onMessageReceived">A function that is invoked when a message is received.</param>
        public static void Start(this IReceiver receiver, OnMessageReceivedDelegate onMessageReceived)
        {
            if (onMessageReceived == null) throw new ArgumentNullException(nameof(onMessageReceived));
            receiver.Start(new DelegateMessageHandler(onMessageReceived));
        }

        /// <summary>
        /// Start listening for messages and handle them using the specified message handler.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="messageHandler">The object that handles received messages.</param>
        public static void Start(this IReceiver receiver, IMessageHandler messageHandler)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));
            if (receiver.MessageHandler != null)
                throw new InvalidOperationException("The receiver is already started.");

            receiver.MessageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        private class DelegateMessageHandler : IMessageHandler
        {
            private readonly OnMessageReceivedDelegate _onMessageReceived;

            public DelegateMessageHandler(OnMessageReceivedDelegate onMessageReceived) =>
                _onMessageReceived = onMessageReceived;

            public void OnMessageReceived(IReceiver receiver, IReceiverMessage message) =>
                _onMessageReceived(receiver, message);
        }
    }
}
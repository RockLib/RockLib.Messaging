using System;
using System.ComponentModel;
using System.Threading.Tasks;

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
        [Obsolete("Use asynchronous Start extension method instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Start(this IReceiver receiver, Action<IReceiverMessage> onMessageReceived)
        {
            if (onMessageReceived is null)
            {
                throw new ArgumentNullException(nameof(onMessageReceived));
            }
            receiver.Start((_, message) => onMessageReceived(message));
        }

        /// <summary>
        /// Start listening for messages and handle them using the specified callback function.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onMessageReceived">A function that is invoked when a message is received.</param>
        [Obsolete("Use asynchronous Start extension method instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Start(this IReceiver receiver, OnMessageReceivedDelegate onMessageReceived)
        {
            if (onMessageReceived is null)
            {
                throw new ArgumentNullException(nameof(onMessageReceived));
            }
            receiver.Start(new SyncDelegateMessageHandler(onMessageReceived));
        }

        /// <summary>
        /// Start listening for messages and handle them using the specified callback function.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onMessageReceivedAsync">A function that is invoked when a message is received.</param>
        public static void Start(this IReceiver receiver, Func<IReceiverMessage, Task> onMessageReceivedAsync)
        {
            if (onMessageReceivedAsync is null)
            {
                throw new ArgumentNullException(nameof(onMessageReceivedAsync));
            }
            receiver.Start((_, message) => onMessageReceivedAsync(message));
        }

        /// <summary>
        /// Start listening for messages and handle them using the specified callback function.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onMessageReceivedAsync">A function that is invoked when a message is received.</param>
        public static void Start(this IReceiver receiver, OnMessageReceivedAsyncDelegate onMessageReceivedAsync)
        {
            if (onMessageReceivedAsync is null)
            {
                throw new ArgumentNullException(nameof(onMessageReceivedAsync));
            }
            receiver.Start(new AsyncDelegateMessageHandler(onMessageReceivedAsync));
        }

        /// <summary>
        /// Start listening for messages and handle them using the specified message handler.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="messageHandler">The object that handles received messages.</param>
        public static void Start(this IReceiver receiver, IMessageHandler messageHandler)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }
            if (receiver.MessageHandler is not null)
            {
                throw new InvalidOperationException("The receiver is already started.");
            }

            receiver.MessageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private class SyncDelegateMessageHandler : IMessageHandler
        {
            private readonly OnMessageReceivedDelegate _onMessageReceived;

            public SyncDelegateMessageHandler(OnMessageReceivedDelegate onMessageReceived) =>
                _onMessageReceived = onMessageReceived;

            public Task OnMessageReceivedAsync(IReceiver receiver, IReceiverMessage message)
            {
                var source = new TaskCompletionSource<int>();

                try
                {
                    _onMessageReceived(receiver, message);
                    source.SetResult(0);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    source.SetException(ex);
                }

                return source.Task;
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        private class AsyncDelegateMessageHandler : IMessageHandler
        {
            private readonly OnMessageReceivedAsyncDelegate _onMessageReceivedAsync;

            public AsyncDelegateMessageHandler(OnMessageReceivedAsyncDelegate onMessageReceivedAsync) =>
                _onMessageReceivedAsync = onMessageReceivedAsync;

            public Task OnMessageReceivedAsync(IReceiver receiver, IReceiverMessage message) =>
                _onMessageReceivedAsync(receiver, message);
        }
    }
}
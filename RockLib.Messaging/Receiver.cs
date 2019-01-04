using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// A base class for implementations of the <see cref="IReceiver"/> interface.
    /// </summary>
    public abstract class Receiver : IReceiver
    {
        private bool _disposed;
        private IMessageHandler _messageHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="Receiver"/> class.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        protected Receiver(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="Receiver"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the message handler for this receiver. When set, the receiver is started
        /// and will invoke the value's <see cref="IMessageHandler.OnMessageReceivedAsync"/> method
        /// when messages are received.
        /// </summary>
        public IMessageHandler MessageHandler
        {
            get => _messageHandler;
            set
            {
                if (_messageHandler != null)
                    throw new InvalidOperationException("The receiver is already started.");

                _messageHandler = value ?? throw new ArgumentNullException(nameof(value));
                Start();
            }
        }

        /// <summary>
        /// Occurs when a connection is established.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Occurs when a connection is lost.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Occurs when an error happens.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Frees resources and stops receiving messages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start listening for messages.
        /// </summary>
        protected abstract void Start();

        /// <summary>
        /// Invokes the <see cref="Connected"/> event.
        /// </summary>
        protected void OnConnected() => Connected?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Invokes the <see cref="Disconnected"/> event.
        /// </summary>
        /// <param name="errorMessage">The error message that describes the reason for the disconnection.</param>
        protected void OnDisconnected(string errorMessage) => Disconnected?.Invoke(this, new DisconnectedEventArgs(errorMessage));

        /// <summary>
        /// Invokes the <see cref="Error"/> event.
        /// </summary>
        /// <param name="message">A message the describes the error.</param>
        /// <param name="exception">The exception responsible for the error.</param>
        protected void OnError(string message, Exception exception) => Error?.Invoke(this, new ErrorEventArgs(message, exception));

        /// <summary>
        /// Unregisters all event handlers from the <see cref="Connected"/>,
        /// <see cref="Disconnected"/>, and <see cref="Error"/> events and sets
        /// <see cref="MessageHandler"/> to null.
        /// </summary>
        /// <param name="disposing">
        /// True if called from the <see cref="Dispose()"/> method, false if called
        /// from the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Connected = null;
                Disconnected = null;
                Error = null;
                _messageHandler = null;
            }

            _disposed = true;
        }
    }
}
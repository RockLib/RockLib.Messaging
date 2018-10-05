using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// A base class for implementations of the <see cref="IReceiver"/> interface.
    /// </summary>
    public abstract class Receiver : IReceiver
    {
        private bool disposed;
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
        /// Gets or sets the message handler for this receiver. When set for the
        /// first time, this receiver starts receiving messages.
        /// </summary>
        public IMessageHandler MessageHandler
        {
            get => _messageHandler;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (ReferenceEquals(_messageHandler, value))
                    return;

                var isFirstTime = _messageHandler == null;

                _messageHandler = value;

                if (isFirstTime)
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
        /// Unregisters all event handlers from the <see cref="Connected"/> and
        /// <see cref="Disconnected"/> events a sets <see cref="MessageHandler"/>
        /// to null.
        /// </summary>
        /// <param name="disposing">
        /// True if called from the <see cref="Dispose()"/> method, false if called
        /// from the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Connected = null;
                Disconnected = null;
                _messageHandler = null;
            }

            disposed = true;
        }
    }
}
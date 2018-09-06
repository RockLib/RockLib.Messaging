using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines an interface for receiving messages.
    /// </summary>
    public interface IReceiver : IDisposable
    {
        /// <summary>
        /// Gets the name of this instance of <see cref="IReceiver"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Starts listening for messages.
        /// </summary>
        /// <param name="selector">Also known as a 'routing key' or 'filter', this value enables only certain messages to be received.</param>
        void Start(string selector = null);

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Occurs when a connection is established.
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// Occurs when a connection is lost.
        /// </summary>
        event EventHandler<DisconnectedEventArgs> Disconnected;
    }
}
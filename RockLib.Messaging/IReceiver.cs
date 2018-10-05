using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines an interface for receiving messages. To start receiving messages,
    /// set the value of the <see cref="MessageHandler"/> property.
    /// </summary>
    public interface IReceiver : IDisposable
    {
        /// <summary>
        /// Gets the name of this instance of <see cref="IReceiver"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the message handler for this receiver. When set for the
        /// first time, this receiver starts receiving messages.
        /// </summary>
        IMessageHandler MessageHandler { get; set; }

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
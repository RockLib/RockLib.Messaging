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
        /// Gets or sets the message handler for this receiver. When set, the receiver is started
        /// and will invoke the value's <see cref="IMessageHandler.OnMessageReceivedAsync"/> method
        /// when messages are received.
        /// </summary>
        /// <remarks>
        /// Implementions of this interface should not allow this property to be set to null or
        /// to be set more than once.
        /// </remarks>
        IMessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Occurs when a connection is established.
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// Occurs when a connection is lost.
        /// </summary>
        event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Occurs when an error happens.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        event EventHandler<ErrorEventArgs> Error;
#pragma warning restore CA1716 // Identifiers should not match keywords
    }
}
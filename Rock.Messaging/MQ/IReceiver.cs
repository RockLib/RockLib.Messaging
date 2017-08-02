using System;
using Rock.Messaging;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
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
        /// <param name="selector">Also known as a 'routing key', this value enables only certain messages to be received.</param>
        void Start(string selector);

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
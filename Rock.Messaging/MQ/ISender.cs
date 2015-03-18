using System;

namespace Rock.Messaging
{
    /// <summary>
    /// Defines an interface for sending messages.
    /// </summary>
    public interface ISender : IDisposable
    {
        /// <summary>
        /// Gets the name of this instance of <see cref="ISender"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void Send(ISenderMessage message);
    }
}
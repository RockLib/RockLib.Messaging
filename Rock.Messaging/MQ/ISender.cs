using System;
using System.Threading.Tasks;

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
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        Task SendAsync(ISenderMessage message);
    }
}
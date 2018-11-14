using System;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging
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
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        Task SendAsync(SenderMessage message, CancellationToken cancellationToken);
    }
}
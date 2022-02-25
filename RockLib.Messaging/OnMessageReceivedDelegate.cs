using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines a synchronous function that handles a received message.
    /// </summary>
    /// <param name="receiver">The instance of <see cref="IReceiver"/> that received the message.</param>
    /// <param name="message">The message that was received.</param>
    [Obsolete("Use OnMessageReceivedAsyncDelegate instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public delegate void OnMessageReceivedDelegate(IReceiver receiver, IReceiverMessage message);
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

    /// <summary>
    /// Defines an asynchronous function that handles a received message.
    /// </summary>
    /// <param name="receiver">The instance of <see cref="IReceiver"/> that received the message.</param>
    /// <param name="message">The message that was received.</param>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public delegate Task OnMessageReceivedAsyncDelegate(IReceiver receiver, IReceiverMessage message);
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
}

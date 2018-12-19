using System.Threading.Tasks;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines a synchronous function that handles a received message.
    /// </summary>
    /// <param name="receiver">The instance of <see cref="IReceiver"/> that received the message.</param>
    /// <param name="message">The message that was received.</param>
    public delegate void OnMessageReceivedDelegate(IReceiver receiver, IReceiverMessage message);

    /// <summary>
    /// Defines an asynchronous function that handles a received message.
    /// </summary>
    /// <param name="receiver">The instance of <see cref="IReceiver"/> that received the message.</param>
    /// <param name="message">The message that was received.</param>
    public delegate Task OnMessageReceivedAsyncDelegate(IReceiver receiver, IReceiverMessage message);
}

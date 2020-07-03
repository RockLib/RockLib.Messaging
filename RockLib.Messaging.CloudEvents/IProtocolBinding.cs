namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines how to bind a CloudEvent attribute name to a <see cref="SenderMessage"/> or
    /// <see cref="IReceiverMessage"/> header name.
    /// </summary>
    public interface IProtocolBinding
    {
        /// <summary>
        /// Gets the header name for a <see cref="SenderMessage"/> or <see cref="IReceiverMessage"/>
        /// given the specified CloudEvent attribute name.
        /// </summary>
        /// <param name="headerKey">The CloudEvent attribute name.</param>
        /// <returns>The header name for a <see cref="SenderMessage"/>.</returns>
        string GetHeaderName(string headerKey);
    }
}

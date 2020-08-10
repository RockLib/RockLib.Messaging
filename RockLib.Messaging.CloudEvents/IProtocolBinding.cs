namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines how to bind a CloudEvent attribute name to a <see cref="SenderMessage"/> or
    /// <see cref="IReceiverMessage"/> header name.
    /// </summary>
    public interface IProtocolBinding
    {
        /// <summary>
        /// Gets the header name for a <see cref="SenderMessage"/> given the specified CloudEvent
        /// attribute name.
        /// </summary>
        /// <param name="attributeName">The CloudEvent attribute name.</param>
        /// <returns>The header name for a <see cref="SenderMessage"/>.</returns>
        string GetHeaderName(string attributeName);

        /// <summary>
        /// Gets the attribute name for a <see cref="CloudEvent"/> given the specified <see cref=
        /// "IReceiverMessage"/> header name.
        /// </summary>
        /// <param name="headerName">The <see cref="IReceiverMessage"/> header name.</param>
        /// <param name="isCloudEventAttribute">
        /// When this method returns, a value indicating whether <paramref name="headerName"/>
        /// represents a CloudEvent attribute according to this protocol binding. A value of <see
        /// langword="false"/> indicates that <paramref name="headerName"/> represents some
        /// non-CloudEvent attribute.
        /// </param>
        /// <returns>The CloudEvent attribute name.</returns>
        string GetAttributeName(string headerName, out bool isCloudEventAttribute);

        /// <summary>
        /// Binds the attributes of the <see cref="CloudEvent"/> to the headers of the <see cref=
        /// "SenderMessage"/>. Called after <see cref="CloudEvent.Attributes"/> has been
        /// mapped to <see cref="SenderMessage.Headers"/>.
        /// </summary>
        /// <param name="fromCloudEvent">The source <see cref="CloudEvent"/>.</param>
        /// <param name="toSenderMessage">The target <see cref="SenderMessage"/>.</param>
        void Bind(CloudEvent fromCloudEvent, SenderMessage toSenderMessage);

        /// <summary>
        /// Binds the headers of the <see cref="IReceiverMessage"/> to the attributes of the <see
        /// cref="CloudEvent"/>. Called after all of the headers of the <see cref=
        /// "IReceiverMessage"/> have been added to <see cref="CloudEvent.Attributes"/>,
        /// or <see cref="CloudEvent.Headers"/>.
        /// </summary>
        /// <param name="fromReceiverMessage">The source <see cref="IReceiverMessage"/>.</param>
        /// <param name="toCloudEvent">The target <see cref="CloudEvent"/>.</param>
        void Bind(IReceiverMessage fromReceiverMessage, CloudEvent toCloudEvent);
    }
}

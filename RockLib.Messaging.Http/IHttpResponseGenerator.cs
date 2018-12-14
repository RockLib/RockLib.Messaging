namespace RockLib.Messaging.Http
{
    /// <summary>
    /// Defines an object that generates http responses for a specified message.
    /// </summary>
    public interface IHttpResponseGenerator
    {
        /// <summary>
        /// Get an http response for an http request when the user has acknowledged
        /// it.
        /// </summary>
        /// <param name="message">The message that is being acknowledged.</param>
        HttpResponse GetAcknowledgeResponse(HttpListenerReceiverMessage message);

        /// <summary>
        /// Get an http response for an http request when the user has rolled it
        /// back.
        /// </summary>
        /// <param name="message">The message that is being rolled back.</param>
        HttpResponse GetRollbackResponse(HttpListenerReceiverMessage message);

        /// <summary>
        /// Get an http response for an http request when the user has rejected it.
        /// </summary>
        /// <param name="message">The message that is being rejected.</param>
        HttpResponse GetRejectResponse(HttpListenerReceiverMessage message);
    }
}

namespace RockLib.Messaging.Http
{
    /// <summary>
    /// A simple implementation of <see cref="IHttpResponseGenerator"/>.
    /// </summary>
    public class DefaultHttpResponseGenerator : IHttpResponseGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpResponseGenerator"/> class.
        /// </summary>
        /// <param name="acknowledgeStatusCode">
        /// The status code to be returned to the client when a message is acknowledged.
        /// </param>
        /// <param name="rollbackStatusCode">
        /// The status code to be returned to the client when a message is rolled back.
        /// </param>
        /// <param name="rejectStatusCode">
        /// The status code to be returned to the client when a message is acknowledged.
        /// </param>
        public DefaultHttpResponseGenerator(int acknowledgeStatusCode, int rollbackStatusCode, int rejectStatusCode)
        {
            AcknowledgeResponse = new HttpResponse(acknowledgeStatusCode);
            RollbackResponse = new HttpResponse(rollbackStatusCode);
            RejectResponse = new HttpResponse(rejectStatusCode);
        }

        /// <summary>
        /// Gets the response to be returned from calls to <see cref="GetAcknowledgeResponse"/>.
        /// </summary>
#pragma warning disable CA1721 // Property names should not match get methods
        public HttpResponse AcknowledgeResponse { get; }
#pragma warning restore CA1721 // Property names should not match get methods

        /// <summary>
        /// Gets the response to be returned from calls to <see cref="GetRollbackResponse"/>.
        /// </summary>
#pragma warning disable CA1721 // Property names should not match get methods
        public HttpResponse RollbackResponse { get; }
#pragma warning restore CA1721 // Property names should not match get methods

        /// <summary>
        /// Gets the response to be returned from calls to <see cref="GetRejectResponse"/>.
        /// </summary>
#pragma warning disable CA1721 // Property names should not match get methods
        public HttpResponse RejectResponse { get; }
#pragma warning restore CA1721 // Property names should not match get methods

        /// <summary>
        /// Returns <see cref="AcknowledgeResponse"/>.
        /// </summary>
        public HttpResponse GetAcknowledgeResponse(HttpListenerReceiverMessage message) => AcknowledgeResponse;

        /// <summary>
        /// Returns <see cref="RollbackResponse"/>.
        /// </summary>
        public HttpResponse GetRollbackResponse(HttpListenerReceiverMessage message) => RollbackResponse;

        /// <summary>
        /// Returns <see cref="RejectResponse"/>.
        /// </summary>
        public HttpResponse GetRejectResponse(HttpListenerReceiverMessage message) => RejectResponse;
    }
}

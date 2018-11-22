namespace RockLib.Messaging.Http
{
    public class DefaultHttpResponseGenerator : IHttpResponseGenerator
    {
        public DefaultHttpResponseGenerator(
            int acknowledgeStatusCode, string acknowledgeStatusDescription,
            int rollbackStatusCode, string rollbackStatusDescription,
            int rejectStatusCode, string rejectStatusDescription)
        {
            AcknowledgeResponse = new HttpResponse(acknowledgeStatusCode, acknowledgeStatusDescription);
            RollbackResponse = new HttpResponse(rollbackStatusCode, rollbackStatusDescription);
            RejectResponse = new HttpResponse(rejectStatusCode, rejectStatusDescription);
        }

        public HttpResponse AcknowledgeResponse { get; }
        public HttpResponse RollbackResponse { get; }
        public HttpResponse RejectResponse { get; }

        public HttpResponse GetAcknowledgeResponse(HttpListenerReceiverMessage message) => AcknowledgeResponse;
        public HttpResponse GetRollbackResponse(HttpListenerReceiverMessage message) => RollbackResponse;
        public HttpResponse GetRejectResponse(HttpListenerReceiverMessage message) => RejectResponse;
    }
}

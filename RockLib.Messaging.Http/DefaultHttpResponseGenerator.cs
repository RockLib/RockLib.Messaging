namespace RockLib.Messaging.Http
{
    public class DefaultHttpResponseGenerator : IHttpResponseGenerator
    {
        public DefaultHttpResponseGenerator(int acknowledgeStatusCode, string acknowledgeStatusDescription,
            int rollbackStatusCode, string rollbackStatusDescription)
        {
            AcknowledgeResponse = new HttpResponse(acknowledgeStatusCode, acknowledgeStatusDescription);
            RollbackResponse = new HttpResponse(rollbackStatusCode, rollbackStatusDescription);
        }

        public HttpResponse AcknowledgeResponse { get; }
        public HttpResponse RollbackResponse { get; }

        public HttpResponse GetAcknowledgeResponse(HttpListenerReceiverMessage message) => AcknowledgeResponse;
        public HttpResponse GetRollbackResponse(HttpListenerReceiverMessage message) => RollbackResponse;
    }
}

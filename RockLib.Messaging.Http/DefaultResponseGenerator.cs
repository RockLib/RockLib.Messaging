namespace RockLib.Messaging.Http
{
    public class DefaultResponseGenerator : IResponseGenerator
    {
        public DefaultResponseGenerator(int acknowledgeStatusCode, string acknowledgeStatusDescription,
            int rollbackStatusCode, string rollbackStatusDescription)
        {
            AcknowledgeResponse = new Response(acknowledgeStatusCode, acknowledgeStatusDescription);
            RollbackResponse = new Response(rollbackStatusCode, rollbackStatusDescription);
        }

        public Response AcknowledgeResponse { get; }
        public Response RollbackResponse { get; }

        public Response GetAcknowledgeResponse(HttpListenerReceiverMessage message) => AcknowledgeResponse;
        public Response GetRollbackResponse(HttpListenerReceiverMessage message) => RollbackResponse;
    }
}

namespace RockLib.Messaging.Http
{
    public interface IResponseGenerator
    {
        Response GetAcknowledgeResponse(HttpListenerReceiverMessage message);
        Response GetRollbackResponse(HttpListenerReceiverMessage message);
    }
}

namespace RockLib.Messaging.Http
{
    public interface IHttpResponseGenerator
    {
        HttpResponse GetAcknowledgeResponse(HttpListenerReceiverMessage message);
        HttpResponse GetRollbackResponse(HttpListenerReceiverMessage message);
    }
}

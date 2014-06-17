using System;

namespace Rock.Messaging.Routing
{
    public interface IMessageRouter
    {
        void Route(
            string rawMessage,
            Action<IMessage, object> onSuccess = null,
            Action<Exception> onFailure = null,
            Action onComplete = null);
    }
}
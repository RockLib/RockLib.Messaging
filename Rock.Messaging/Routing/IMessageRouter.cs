using System;

namespace Rock.Messaging.Routing
{
    public interface IMessageRouter
    {
        void Route(string rawMessage, Action<RouteResult> onComplete = null);
    }
}
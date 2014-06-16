using System;
using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    public interface IMessageRouter
    {
        Task<RouteResult> Route(string rawMessage);
    }
}
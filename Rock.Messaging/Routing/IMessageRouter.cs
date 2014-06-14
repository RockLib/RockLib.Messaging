using System;
using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    public interface IMessageRouter
    {
        Task Route(string rawMessage, Action<IRouteResult> completion = null);
    }
}
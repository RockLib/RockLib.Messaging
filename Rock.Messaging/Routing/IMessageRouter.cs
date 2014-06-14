using System;
using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    public interface IMessageRouter
    {
        Task<IMessage> Route(string rawMessage);
    }
}
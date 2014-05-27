using System;

namespace Rock.Messaging.Routing
{
    public interface ITypeLocator
    {
        Type GetMessageType(string typeName);
        Type GetMessageHandlerType(Type messageType);
    }
}
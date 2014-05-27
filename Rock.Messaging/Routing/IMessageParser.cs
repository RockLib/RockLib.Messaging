using System;

namespace Rock.Messaging.Routing
{
    public interface IMessageParser
    {
        string GetTypeName(Type type);
        string GetTypeName(string rawMessage);
        object DeserializeMessage(string rawMessage, Type messageType);
    }
}
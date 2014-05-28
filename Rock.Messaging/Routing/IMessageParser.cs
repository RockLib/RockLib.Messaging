using System;

namespace Rock.Messaging.Routing
{
    public interface IMessageParser
    {
        string GetTypeName(Type type);
        string GetTypeName(string rawMessage);
        TMessage DeserializeMessage<TMessage>(string rawMessage)
            where TMessage : IMessage;
    }
}
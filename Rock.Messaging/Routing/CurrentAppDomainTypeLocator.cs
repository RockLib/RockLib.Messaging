using System;
using System.Linq;

namespace Rock.Messaging.Routing
{
    public class CurrentAppDomainTypeLocator : ITypeLocator
    {
        private readonly IMessageParser _messageParser;

        public CurrentAppDomainTypeLocator(IMessageParser messageParser)
        {
            _messageParser = messageParser;
        }

        public Type GetMessageType(string typeName)
        {
            return
               (from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                where typeof(IMessage).IsAssignableFrom(t) && _messageParser.GetTypeName(t) == typeName
                select t).Single();
        }

        public Type GetMessageHandlerType(Type messageType)
        {
            return
               (from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                from i in t.GetInterfaces()
                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>) && i.GetGenericArguments()[0] == messageType
                select t).Single();
        }
    }
}
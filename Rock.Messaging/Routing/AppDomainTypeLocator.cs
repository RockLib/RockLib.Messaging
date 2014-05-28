using System;
using System.Linq;

namespace Rock.Messaging.Routing
{
    public class AppDomainTypeLocator : ITypeLocator
    {
        private readonly IMessageParser _messageParser;
        private readonly AppDomain _appDomain;

        public AppDomainTypeLocator(IMessageParser messageParser, AppDomain appDomain = null)
        {
            _messageParser = messageParser;
            _appDomain = appDomain ?? AppDomain.CurrentDomain;
        }

        public Type GetMessageType(string typeName)
        {
            return
               (from a in _appDomain.GetAssemblies()
                from t in a.GetTypes()
                where !t.IsAbstract && typeof(IMessage).IsAssignableFrom(t) && _messageParser.GetTypeName(t) == typeName
                select t).Single();
        }

        public Type GetMessageHandlerType(Type messageType)
        {
            return
               (from a in _appDomain.GetAssemblies()
                from t in a.GetTypes()
                where !t.IsAbstract
                from i in t.GetInterfaces()
                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>) && i.GetGenericArguments()[0] == messageType
                select t).Single();
        }
    }
}
using System;
using System.Linq;
using Rock.Reflection;

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
                from t in a.GetTypesSafely()
                where !t.IsAbstract && _messageParser.GetTypeName(t) == typeName
                      && GetMessageHandlerTypeImpl(t) != null
                select t).Single();
        }

        public Type GetMessageHandlerType(Type messageType)
        {
            var messageHandlerType = GetMessageHandlerTypeImpl(messageType);

            if (messageHandlerType == null)
            {
                throw new InvalidOperationException("No message handler found for type " + messageType.FullName);
            }

            return messageHandlerType;
        }

        private Type GetMessageHandlerTypeImpl(Type messageType)
        {
            try
            {
                return
                   (from a in _appDomain.GetAssemblies()
                    from t in a.GetTypesSafely()
                    where !t.IsAbstract
                    from i in t.GetInterfaces()
                    where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>) && i.GetGenericArguments()[0] == messageType
                    select t).Single();
            }
            catch
            {
                return null;
            }
        }
    }
}
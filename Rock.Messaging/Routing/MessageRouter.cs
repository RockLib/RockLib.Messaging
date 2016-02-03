using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Rock.DependencyInjection;

namespace Rock.Messaging.Routing
{
    public class MessageRouter : IMessageRouter
    {
        private readonly ConcurrentDictionary<string, Func<string, Task<HandleResult>>> _handleFunctions = new ConcurrentDictionary<string, Func<string, Task<HandleResult>>>();

        private readonly IMessageParser _messageParser;
        private readonly ITypeLocator _typeLocator;
        private readonly IResolver _resolver;

        // ReSharper disable RedundantArgumentDefaultValue
        public MessageRouter()
            : this(null, null, null)
        {
        }
        // ReSharper restore RedundantArgumentDefaultValue

        public MessageRouter(
            IMessageParser messageParser = null,
            ITypeLocator typeLocator = null,
            IResolver resolver = null)
        {
            _messageParser = messageParser ?? DefaultMessageParser.Current;
            _typeLocator = typeLocator ?? DefaultTypeLocator.Current;
            _resolver = resolver ?? new AutoContainer();
        }

        public async void Route(string rawMessage, Action<RouteResult> onComplete = null)
        {
            try
            {
                var handleMessage =
                    _handleFunctions.GetOrAdd(
                        _messageParser.GetTypeName(rawMessage),
                        rootElement => GetHandleMessageFunc(rootElement));

                var handleResult = await handleMessage(rawMessage);

                if (onComplete != null)
                {
                    try
                    {
                        onComplete(new RouteResult(handleResult.Message, handleResult.Result));
                    } // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                if (onComplete != null)
                {
                    try
                    {
                        onComplete(new RouteResult(ex));
                    } // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
            }
        }

        private Func<string, Task<HandleResult>> GetHandleMessageFunc(string rootElement)
        {
            var messageType = _typeLocator.GetMessageType(rootElement);

            var genericMessageHandlerType = typeof(GenericMessageHandler<>).MakeGenericType(messageType);
            var genericMessageHandler = Activator.CreateInstance(genericMessageHandlerType, _typeLocator, _resolver, _messageParser);

            var methodInfo = genericMessageHandlerType.GetMethod("HandleMessage");

            var delegateType = typeof(Func<string, Task<HandleResult>>);

            return (Func<string, Task<HandleResult>>)Delegate.CreateDelegate(delegateType, genericMessageHandler, methodInfo);
        }

        private class GenericMessageHandler<TMessage>
        {
            private readonly IResolver _resolver;
            private readonly IMessageParser _messageParser;
            private readonly Type _messageHandlerType;

            public GenericMessageHandler(ITypeLocator typeLocator, IResolver resolver, IMessageParser messageParser)
            {
                _resolver = resolver;
                _messageParser = messageParser;
                _messageHandlerType = typeLocator.GetMessageHandlerType(typeof(TMessage));
            }

            // ReSharper disable once UnusedMember.Local
            public async Task<HandleResult> HandleMessage(string rawMessage)
            {
                var messageHandler = (IMessageHandler<TMessage>)_resolver.Get(_messageHandlerType);

                var message = _messageParser.DeserializeMessage<TMessage>(rawMessage);
                var result = await messageHandler.Handle(message);

                return new HandleResult(message, result);
            }
        }

        private class HandleResult
        {
            private readonly object _message;
            private readonly object _result;

            public HandleResult(object message, object result)
            {
                _message = message;
                _result = result;
            }

            public object Message
            {
                get { return _message; }
            }

            public object Result
            {
                get { return _result; }
            }
        }
    }
}
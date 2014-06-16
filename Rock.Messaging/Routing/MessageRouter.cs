using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Rock.Defaults;
using Rock.DependencyInjection;
using Rock.Messaging.Defaults.Implementation;

namespace Rock.Messaging.Routing
{
    public class MessageRouter : IMessageRouter
    {
        private readonly ConcurrentDictionary<string, Func<string, Task<IMessage>>> _routeFunctions = new ConcurrentDictionary<string, Func<string, Task<IMessage>>>();

        private readonly IMessageParser _messageParser;
        private readonly ITypeLocator _typeLocator;
        private readonly IResolver _resolver;

        // ReSharper disable RedundantArgumentDefaultValue
        public MessageRouter()
            : this(null, null, null)
        {
        }
        // ReSharper restore RedundantArgumentDefaultValue

        [UsesDefaultValue(typeof(Default), "MessageParser")]
        [UsesDefaultValue(typeof(Default), "TypeLocator")]
        [UsesDefaultValue(typeof(Default), "ExceptionHandler")]
        public MessageRouter(
            IMessageParser messageParser = null,
            ITypeLocator typeLocator = null,
            IResolver resolver = null)
        {
            _messageParser = messageParser ?? Default.MessageParser;
            _typeLocator = typeLocator ?? Default.TypeLocator;
            _resolver = resolver ?? new AutoContainer();
        }

        public async Task<RouteResult> Route(string rawMessage)
        {
            try
            {
                var routeFunction =
                    _routeFunctions.GetOrAdd(
                        _messageParser.GetTypeName(rawMessage),
                        rootElement => CreateRouteFunction(rootElement));

                var message = await routeFunction(rawMessage);
                return new RouteResult(message);
            }
            catch (Exception ex)
            {
                return new RouteResult(ex);
            }
        }

        private Func<string, Task<IMessage>> CreateRouteFunction(string rootElement)
        {
            var messageType = _typeLocator.GetMessageType(rootElement);
            var rawMessageParameter = Expression.Parameter(typeof(string), "rawMessage");

            var messageParserDeserializeMessageMethod = typeof(IMessageParser).GetMethod("DeserializeMessage").MakeGenericMethod(messageType);
            var deserializeExpression =
                Expression.Call(
                    Expression.Constant(_messageParser),
                    messageParserDeserializeMessageMethod,
                    new Expression[]{ rawMessageParameter });

            var messageHandlerType = _typeLocator.GetMessageHandlerType(messageType);
            var handleMethod = messageHandlerType.GetMethod("Handle");

            var resolverGetMethod = typeof(IResolver).GetMethod("Get", Type.EmptyTypes).MakeGenericMethod(messageHandlerType);
            var getMessageHandlerExpression = Expression.Call(Expression.Constant(_resolver), resolverGetMethod);

            var body = Expression.Call(getMessageHandlerExpression, handleMethod, new Expression[] { deserializeExpression });

            var lambda =
                Expression.Lambda<Func<string, Task<IMessage>>>(
                    body,
                    "Route" + messageType.Name,
                    new[] { rawMessageParameter });
            return lambda.Compile();
        }
    }
}
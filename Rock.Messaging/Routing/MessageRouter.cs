using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Rock.Defaults;
using Rock.DependencyInjection;
using Rock.Messaging.Defaults.Implementation;

namespace Rock.Messaging.Routing
{
    public class MessageRouter : IMessageRouter
    {
        private static readonly MethodInfo _genericMessageParserDeserializeMessageMethod;
        private static readonly MethodInfo _genericResolverGetMethod;
        private static readonly MethodInfo _getContinuationFunctionMethod;
        private static readonly MethodInfo _continueWithMethod;

        private readonly ConcurrentDictionary<string, Func<string, Task<HandleResult>>> _handleFunctions = new ConcurrentDictionary<string, Func<string, Task<HandleResult>>>();

        private readonly IMessageParser _messageParser;
        private readonly ITypeLocator _typeLocator;
        private readonly IResolver _resolver;

        static MessageRouter()
        {
            _genericMessageParserDeserializeMessageMethod = typeof(IMessageParser).GetMethod("DeserializeMessage");
            _genericResolverGetMethod = typeof(IResolver).GetMethod("Get", Type.EmptyTypes);
            _getContinuationFunctionMethod = typeof(MessageRouter).GetMethod("GetContinuationFunction", BindingFlags.NonPublic | BindingFlags.Static);

            _continueWithMethod =
                (from m in typeof(Task<object>).GetMethods()
                    where
                        m.Name == "ContinueWith"
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 1
                    let p = m.GetParameters()[0].ParameterType
                    where
                        p.IsGenericType
                        && p.GetGenericTypeDefinition() == typeof(Func<,>)
                    let args = p.GetGenericArguments()
                    where
                        args[0] == typeof(Task<object>)
                        && args[1].IsGenericParameter
                    select m)
                .Single()
                .MakeGenericMethod(typeof(HandleResult));
        }

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

        public async void Route(
            string rawMessage,
            Action<IMessage, object> onSuccess = null,
            Action<Exception> onFailure = null,
            Action onComplete = null)
        {
            try
            {
                var handle =
                    _handleFunctions.GetOrAdd(
                        _messageParser.GetTypeName(rawMessage),
                        rootElement => CreateRouteFunction(rootElement));

                var handleResult = await handle(rawMessage);

                if (onSuccess != null)
                {
                    try
                    {
                        onSuccess(handleResult.Message, handleResult.Result);
                    } // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                if (onFailure != null)
                {
                    try
                    {
                        onFailure(ex);
                    } // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
            }

            if (onComplete != null)
            {
                try
                {
                    onComplete();
                } // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }
        }

        private Func<string, Task<HandleResult>> CreateRouteFunction(string rootElement)
        {
            var messageType = _typeLocator.GetMessageType(rootElement);
            var rawMessageParameter = Expression.Parameter(typeof(string), "rawMessage");

            var messageParserDeserializeMessageMethod = _genericMessageParserDeserializeMessageMethod.MakeGenericMethod(messageType);
            var callDeserializeMethod = Expression.Call(Expression.Constant(_messageParser), messageParserDeserializeMessageMethod, new Expression[] { rawMessageParameter });

            var messageHandlerType = _typeLocator.GetMessageHandlerType(messageType);
            var handleMethod = messageHandlerType.GetMethod("Handle");

            var resolverGetMethod = _genericResolverGetMethod.MakeGenericMethod(messageHandlerType);
            var callResolverGetMessageHandler = Expression.Call(Expression.Constant(_resolver), resolverGetMethod);

            var messageVariable = Expression.Variable(messageType, "message");

            var assignMessageVariable = Expression.Assign(messageVariable, callDeserializeMethod);

            var callHandleMethod = Expression.Call(callResolverGetMessageHandler, handleMethod, new Expression[] { messageVariable });

            var callGetContinuationFunctionMethod = Expression.Call(_getContinuationFunctionMethod, messageVariable);

            var callContinueWithMethod = Expression.Call(callHandleMethod, _continueWithMethod, new Expression[] { callGetContinuationFunctionMethod });

            var body =
                Expression.Block(
                    typeof(Task<HandleResult>),
                    new[] { messageVariable },
                    assignMessageVariable,
                    callContinueWithMethod);

            var lambda =
                Expression.Lambda<Func<string, Task<HandleResult>>>(
                    body,
                    "Route" + messageType.Name,
                    new[] { rawMessageParameter });
            return lambda.Compile();
        }

        // ReSharper disable once UnusedMember.Local
        private static Func<Task<object>, HandleResult> GetContinuationFunction(IMessage message)
        {
            return task => new HandleResult(message, task.Result);
        }

        private class HandleResult
        {
            private readonly IMessage _message;
            private readonly object _result;

            public HandleResult(IMessage message, object result)
            {
                _message = message;
                _result = result;
            }

            public IMessage Message
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
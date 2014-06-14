using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Rock.Defaults;
using Rock.DependencyInjection;
using Rock.Messaging.Defaults.Implementation;

namespace Rock.Messaging.Routing
{
    public interface IRouteResult
    {
        bool Success { get; }
        IMessage Message { get; }
        Exception Exception { get; }
    }

    public class RouteResult : IRouteResult
    {
        public bool Success
        {
            get { return Message != null; }
        }

        public IMessage Message { get; set; }

        public Exception Exception { get; set; }
    }

    public class MessageRouter : IMessageRouter
    {
        private readonly ConcurrentDictionary<string, Func<string, Task<IMessage>>> _routeFunctions = new ConcurrentDictionary<string, Func<string, Task<IMessage>>>();

        private readonly IMessageParser _messageParser;
        private readonly ITypeLocator _typeLocator;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IResolver _resolver;

        // ReSharper disable RedundantArgumentDefaultValue
        public MessageRouter()
            : this(null, null, null, null)
        {
        }
        // ReSharper restore RedundantArgumentDefaultValue

        [UsesDefaultValue(typeof(Default), "MessageParser")]
        [UsesDefaultValue(typeof(Default), "TypeLocator")]
        [UsesDefaultValue(typeof(Default), "ExceptionHandler")]
        public MessageRouter(
            IMessageParser messageParser = null,
            ITypeLocator typeLocator = null,
            IExceptionHandler exceptionHandler = null,
            IResolver resolver = null)
        {
            _messageParser = messageParser ?? Default.MessageParser;
            _typeLocator = typeLocator ?? Default.TypeLocator;
            _exceptionHandler = exceptionHandler ?? Default.ExceptionHandler;
            _resolver = resolver ?? new AutoContainer();
        }

        public async Task Route(string rawMessage, Action<IRouteResult> completion = null)
        {
            var routeResult = new RouteResult();

            try
            {
                var routeFunction =
                    _routeFunctions.GetOrAdd(
                        _messageParser.GetTypeName(rawMessage),
                        rootElement => CreateRouteFunction(rootElement));

                routeResult.Message = await routeFunction(rawMessage);
            }
            catch (Exception ex)
            {
                routeResult.Exception = ex;
                HandleException(ex);
            }
            finally
            {
                if (completion != null)
                {
                    try
                    {
                        completion(routeResult);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                }
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
            var getMessageHandlerExpression =
                Expression.Call(
                    Expression.Constant(_resolver),
                    resolverGetMethod);

            var body = Expression.Call(getMessageHandlerExpression, handleMethod, new Expression[] { deserializeExpression });

            var lambda =
                Expression.Lambda<Func<string, Task<IMessage>>>(
                    body,
                    "Route" + messageType.Name,
                    new[] { rawMessageParameter });
            return lambda.Compile();
        }

        private static object GetCompletedTask(Type messageType)
        {
            var taskFromResultMethod = typeof(Task).GetMethod("FromResult").MakeGenericMethod(messageType);
            var completedTask = taskFromResultMethod.Invoke(null, new object[] { null });
            return completedTask;
        }

        // ReSharper disable once EmptyGeneralCatchClause
        private async void HandleException(Exception ex)
        {
            try
            {
                await _exceptionHandler.HandleException(ex);
            }
            catch
            {
            }
        }
    }
}
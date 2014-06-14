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
    public class MessageRouter : IMessageRouter
    {
        private readonly ConcurrentDictionary<string, Func<string, Task>> _routeFunctions = new ConcurrentDictionary<string, Func<string, Task>>();

        private Action _defaultCompletion;
        private readonly ConcurrentDictionary<Type, Delegate> _completions = new ConcurrentDictionary<Type, Delegate>();
        private readonly object _locker = new object();

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

        public async Task Route(string rawMessage, Action completion = null)
        {
            try
            {
                var routeFunction =
                    _routeFunctions.GetOrAdd(
                        _messageParser.GetTypeName(rawMessage),
                        rootElement => CreateRouteFunction(rootElement));

                await routeFunction(rawMessage);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (completion != null)
                {
                    try
                    {
                        completion();
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                }
            }
        }

        public void RegisterDefaultCompletion(Action completion)
        {
            lock (_locker)
            {
                _defaultCompletion = completion;
                _routeFunctions.Clear();
            }
        }

        public void RegisterCompletion<TMessage>(Action<TMessage> completion)
            where TMessage : IMessage
        {
            lock (_locker)
            {
                _completions.AddOrUpdate(typeof(TMessage), t => completion, (t, d) => completion);
                Func<string, Task> dummy;
                _routeFunctions.TryRemove(_messageParser.GetTypeName(typeof(TMessage)), out dummy);
            }
        }

        private Func<string, Task> CreateRouteFunction(string rootElement)
        {
            lock (_locker)
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

                var continueWithMethod =
                    typeof(Task<>).MakeGenericType(messageType).GetMethods()
                        .Single(m =>
                            m.Name == "ContinueWith"
                            && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType == typeof(Action<>).MakeGenericType(typeof(Task<>).MakeGenericType(messageType)));

                var handleExceptionMethod = GetType().GetMethod("HandleException", BindingFlags.NonPublic | BindingFlags.Instance);

                var continueWithExpression = Expression.Constant(CreateContinueWithFunction(messageType, handleExceptionMethod));

                var tryBody =
                    Expression.Call(
                        Expression.Call(getMessageHandlerExpression, handleMethod, new Expression[] { deserializeExpression }),
                        continueWithMethod,
                        new Expression[] { continueWithExpression });

                var exceptionVariable = Expression.Variable(typeof(Exception), "ex");

                var catchBlock =
                    Expression.MakeCatchBlock(
                        typeof(Exception),
                        exceptionVariable,
                        Expression.Block(
                            typeof(Task),
                            Expression.Call(Expression.Constant(this), handleExceptionMethod, new Expression[] { exceptionVariable }),
                            Expression.Constant(GetCompletedTask(messageType))),
                        null);

                var body = Expression.TryCatch(tryBody, catchBlock);

                var lambda =
                    Expression.Lambda<Func<string, Task>>(
                        body,
                        "Route" + messageType.Name,
                        new[] { rawMessageParameter });
                return lambda.Compile();
            }
        }

        private Delegate CreateContinueWithFunction(Type messageType, MethodInfo handleExceptionMethod)
        {
            var invokeCompletionMessageParameter = Expression.Parameter(typeof(Task<>).MakeGenericType(messageType), "messageTask");

            var bodyExpressions = new List<Expression>();

            var handleExceptionExpression =
                Expression.IfThen(
                    Expression.Property(invokeCompletionMessageParameter, "IsFaulted"),
                    Expression.Call(Expression.Constant(this), handleExceptionMethod, new Expression[] { Expression.Property(invokeCompletionMessageParameter, "Exception") }));

            bodyExpressions.Add(handleExceptionExpression);

            Delegate completion;
            if (_completions.TryGetValue(messageType, out completion))
            {
                if (_defaultCompletion != null)
                {
                    bodyExpressions.Add(Try(handleExceptionMethod, GetInvokeGenericCompletionExpression(messageType, completion, invokeCompletionMessageParameter)));
                    bodyExpressions.Add(Try(handleExceptionMethod, GetInvokeDefaultCompletionExpression()));
                }
                else
                {
                    bodyExpressions.Add(Try(handleExceptionMethod, GetInvokeGenericCompletionExpression(messageType, completion, invokeCompletionMessageParameter)));
                }
            }
            else if (_defaultCompletion != null)
            {
                bodyExpressions.Add(Try(handleExceptionMethod, GetInvokeDefaultCompletionExpression()));
            }

            var body = Expression.Block(bodyExpressions);

            var invokeCompletionLambda =
                Expression.Lambda(
                    typeof(Action<>).MakeGenericType(typeof(Task<>).MakeGenericType(messageType)),
                    body,
                    "Route" + messageType.Name + "Continuation",
                    new[] { invokeCompletionMessageParameter });
            return invokeCompletionLambda.Compile();
        }

        private InvocationExpression GetInvokeDefaultCompletionExpression()
        {
            return Expression.Invoke(Expression.Constant(_defaultCompletion));
        }

        private static InvocationExpression GetInvokeGenericCompletionExpression(Type messageType, Delegate completion, ParameterExpression invokeCompletionMessageParameter)
        {
            return Expression.Invoke(
                Expression.Constant(Convert.ChangeType(completion, typeof(Action<>).MakeGenericType(messageType))),
                Expression.Property(invokeCompletionMessageParameter, "Result"));
        }

        private TryExpression Try(MethodInfo handleExceptionMethod, Expression invokeCompletion)
        {
            var exceptionVariable = Expression.Variable(typeof(Exception), "ex");

            var catchBlock =
                Expression.MakeCatchBlock(
                    typeof(Exception),
                    exceptionVariable,
                    Expression.Call(Expression.Constant(this), handleExceptionMethod, new Expression[] {exceptionVariable}),
                    null);

            var tryInvokeCompletion = Expression.TryCatch(invokeCompletion, catchBlock);
            return tryInvokeCompletion;
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
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Rock.Logging;

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
        private readonly ILogger _logger;

        public MessageRouter(IMessageParser messageParser = null, ITypeLocator typeLocator = null, ILogger logger = null)
        {
            _messageParser = messageParser ?? new XmlMessageParser();
            _typeLocator = typeLocator ?? new CurrentAppDomainTypeLocator(_messageParser);
            _logger = logger ?? NullLogger.Instance;
        }

        public Task Route(string rawMessage)
        {
            var routeFunction = _routeFunctions.GetOrAdd(
                _messageParser.GetTypeName(rawMessage),
                rootElement => CreateRouteFunction(rootElement));

            return routeFunction(rawMessage);
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

                var deserializeMethod = GetType().GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Instance);
                var deserializeExpression =
                    Expression.Convert(
                        Expression.Call(Expression.Constant(this), deserializeMethod, rawMessageParameter, Expression.Constant(messageType)),
                        messageType);

                var messageHandlerType = _typeLocator.GetMessageHandlerType(messageType);
                var handleMethod = messageHandlerType.GetMethod("Handle");
                var newMessageHandlerExpression = Expression.New(messageHandlerType);

                var continueWithMethod =
                    typeof(Task<>).MakeGenericType(messageType).GetMethods()
                        .Single(m =>
                            m.Name == "ContinueWith"
                            && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType == typeof(Action<>).MakeGenericType(typeof(Task<>).MakeGenericType(messageType)));

                var logExceptionMethod = GetType().GetMethod("LogException", BindingFlags.NonPublic | BindingFlags.Instance);

                var continueWithExpression = Expression.Constant(CreateContinueWithFunction(messageType, logExceptionMethod));

                var tryBody =
                    Expression.Call(
                        Expression.Call(newMessageHandlerExpression, handleMethod, new Expression[] { deserializeExpression }),
                        continueWithMethod,
                        new Expression[] { continueWithExpression });

                var exceptionVariable = Expression.Variable(typeof(Exception), "ex");

                var catchBlock =
                    Expression.MakeCatchBlock(
                        typeof(Exception),
                        exceptionVariable,
                        Expression.Block(
                            typeof(Task),
                            Expression.Call(Expression.Constant(this), logExceptionMethod, new Expression[] { exceptionVariable }),
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

        private Delegate CreateContinueWithFunction(Type messageType, MethodInfo logExceptionMethod)
        {
            var invokeCompletionMessageParameter = Expression.Parameter(typeof(Task<>).MakeGenericType(messageType), "messageTask");

            Expression invokeCompletion;
            Delegate completion;
            if (_completions.TryGetValue(messageType, out completion))
            {
                invokeCompletion = Expression.Invoke(
                    Expression.Constant(Convert.ChangeType(completion, typeof(Action<>).MakeGenericType(messageType))),
                    Expression.Property(invokeCompletionMessageParameter, "Result"));
            }
            else if (_defaultCompletion != null)
            {
                invokeCompletion = Expression.Invoke(Expression.Constant(_defaultCompletion));
            }
            else
            {
                invokeCompletion = null;
            }

            Expression invokeCompletionBody;

            if (invokeCompletion != null)
            {
                var exceptionVariable = Expression.Variable(typeof(Exception), "ex");

                var catchBlock =
                    Expression.MakeCatchBlock(
                        typeof(Exception),
                        exceptionVariable,
                        Expression.Call(Expression.Constant(this), logExceptionMethod, new Expression[] { exceptionVariable }),
                        null);

                var tryInvokeCompletion = Expression.TryCatch(invokeCompletion, catchBlock);

                invokeCompletionBody =
                    Expression.IfThenElse(
                        Expression.Property(invokeCompletionMessageParameter, "IsFaulted"),
                        Expression.Call(Expression.Constant(this), logExceptionMethod, new Expression[] { Expression.Property(invokeCompletionMessageParameter, "Exception") }),
                        Expression.IfThen(
                            Expression.Not(Expression.Property(invokeCompletionMessageParameter, "IsCanceled")),
                            tryInvokeCompletion));
            }
            else
            {
                invokeCompletionBody =
                    Expression.IfThen(
                        Expression.Property(invokeCompletionMessageParameter, "IsFaulted"),
                        Expression.Call(Expression.Constant(this), logExceptionMethod, new Expression[] { Expression.Property(invokeCompletionMessageParameter, "Exception") }));
            }

            var invokeCompletionLambda =
                Expression.Lambda(
                    typeof(Action<>).MakeGenericType(typeof(Task<>).MakeGenericType(messageType)),
                    invokeCompletionBody,
                    "Route" + messageType.Name + "Continuation",
                    new[] { invokeCompletionMessageParameter });
            return invokeCompletionLambda.Compile();
        }

        private static object GetCompletedTask(Type messageType)
        {
            var taskFromResultMethod = typeof(Task).GetMethod("FromResult").MakeGenericMethod(messageType);
            var completedTask = taskFromResultMethod.Invoke(null, new object[] { null });
            return completedTask;
        }

        // ReSharper disable once UnusedMember.Local
        private object Deserialize(string rawMessage, Type messageType)
        {
            return _messageParser.DeserializeMessage(rawMessage, messageType);
        }

        // ReSharper disable once UnusedMember.Local
        private void LogException(Exception ex)
        {
            _logger.Error(ex);
        }
    }
}
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace RockLib.Messaging.CloudEvents
{
    partial class CloudEventExtensions
    {
        private class MessageConstructor
        {
            private static readonly Type[] _constructorParameters = new[] { typeof(IReceiverMessage), typeof(IProtocolBinding) };

            private Func<IReceiverMessage, IProtocolBinding, object> _invokeConstructor;

            private MessageConstructor(ConstructorInfo constructor)
            {
                // The initial function uses regular reflection.
                _invokeConstructor = (receiverMessage, protocolBinding) =>
                    constructor.Invoke(new object[] { receiverMessage, protocolBinding });

                // Compile the optimized function in the background.
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var receiverMessageParameter = Expression.Parameter(typeof(IReceiverMessage), "receiverMessage");
                    var protocolBindingParameter = Expression.Parameter(typeof(IProtocolBinding), "protocolBinding");

                    var body = Expression.New(constructor, receiverMessageParameter, protocolBindingParameter);

                    var lamda = Expression.Lambda<Func<IReceiverMessage, IProtocolBinding, object>>(
                        body, receiverMessageParameter, protocolBindingParameter);

                    // Replace the reflection function with a compiled function.
                    _invokeConstructor = lamda.Compile();
                });
            }

            public static MessageConstructor Create(Type type)
            {
                var constructor = GetConstructor(type);
                
                if (constructor is null)
                    return null;

                return new MessageConstructor(constructor);
            }

            public static bool Exists(Type type) =>
                GetConstructor(type) != null;

            public object Invoke(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding) =>
                _invokeConstructor(receiverMessage, protocolBinding);

            private static ConstructorInfo GetConstructor(Type type) =>
                type.GetConstructor(_constructorParameters);
        }
    }
}

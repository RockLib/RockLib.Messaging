using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace RockLib.Messaging.CloudEvents
{
    partial class CloudEventExtensions
    {
        private class ValidateMethod
        {
            private const BindingFlags _publicStaticFlags = BindingFlags.Public | BindingFlags.Static;
            private static readonly Type[] _validateMethodParameters = new[] { typeof(SenderMessage), typeof(IProtocolBinding) };

            private Action<SenderMessage, IProtocolBinding> _invokeValidateMethod;

            private ValidateMethod(MethodInfo validateMethod)
            {
                // The initial function uses regular reflection.
                _invokeValidateMethod = (senderMessage, protocolBinding) =>
                    validateMethod.Invoke(null, new object[] { senderMessage, protocolBinding });

                // Compile the optimized function in the background.
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var senderMessageParameter = Expression.Parameter(typeof(SenderMessage), "senderMessage");
                    var protocolBindingParameter = Expression.Parameter(typeof(IProtocolBinding), "protocolBinding");

                    var body = Expression.Call(validateMethod, senderMessageParameter, protocolBindingParameter);

                    var lamda = Expression.Lambda<Action<SenderMessage, IProtocolBinding>>(
                        body, senderMessageParameter, protocolBindingParameter);

                    // Replace the reflection function with a compiled function.
                    _invokeValidateMethod = lamda.Compile();
                });
            }

            public static ValidateMethod? Create(Type type)
            {
                var validateMethod = GetValidateMethod(type);

                if (validateMethod is null)
                {
                    return null;
                }

                return new ValidateMethod(validateMethod);
            }

            public void Invoke(SenderMessage senderMessage, IProtocolBinding protocolBinding) =>
                _invokeValidateMethod(senderMessage, protocolBinding);

            private static MethodInfo? GetValidateMethod(Type type) =>
                type.GetMethod(nameof(CloudEvent.Validate), _publicStaticFlags, null, _validateMethodParameters, null);
        }
    }
}

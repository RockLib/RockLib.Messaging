using System;
using System.Reflection;

namespace RockLib.Messaging.CloudEvents
{
    partial class CloudEventExtensions
    {
        private sealed class ValidateMethod
        {
            private const BindingFlags _publicStaticFlags = BindingFlags.Public | BindingFlags.Static;
            private static readonly Type[] _validateMethodParameters = new[] { typeof(SenderMessage), typeof(IProtocolBinding) };

            private Action<SenderMessage, IProtocolBinding> _invokeValidateMethod;

            private ValidateMethod(MethodInfo validateMethod)
            {
                // The initial function uses regular reflection.
                _invokeValidateMethod = (senderMessage, protocolBinding) =>
                    validateMethod.Invoke(null, new object[] { senderMessage, protocolBinding });
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

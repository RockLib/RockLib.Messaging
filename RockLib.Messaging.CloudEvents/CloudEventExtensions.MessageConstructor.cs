using System;
using System.Reflection;

namespace RockLib.Messaging.CloudEvents
{
    partial class CloudEventExtensions
    {
        private class MessageConstructor
        {
            private static readonly Type[] _constructorParameters = new[] { typeof(IReceiverMessage), typeof(IProtocolBinding) };

            private Func<IReceiverMessage, IProtocolBinding?, object> _invokeConstructor;

            private MessageConstructor(ConstructorInfo constructor)
            {
                // The initial function uses regular reflection.
                _invokeConstructor = (receiverMessage, protocolBinding) =>
                    constructor.Invoke(new object[] { receiverMessage, protocolBinding! });
            }

            public static MessageConstructor? Create(Type type)
            {
                var constructor = GetConstructor(type);

                if (constructor is null)
                {
                    return null;
                }

                return new MessageConstructor(constructor);
            }

            public static bool Exists(Type type) =>
                GetConstructor(type) is not null;

            public object Invoke(IReceiverMessage receiverMessage, IProtocolBinding? protocolBinding) =>
                _invokeConstructor(receiverMessage, protocolBinding);

            private static ConstructorInfo? GetConstructor(Type type) =>
                type.GetConstructor(_constructorParameters);
        }
    }
}

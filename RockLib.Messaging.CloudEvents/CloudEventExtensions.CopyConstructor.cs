using System;
using System.Reflection;

namespace RockLib.Messaging.CloudEvents
{
    partial class CloudEventExtensions
    {
        private class CopyConstructor
        {
            private Func<CloudEvent, CloudEvent> _invokeConstructor;

            private CopyConstructor(ConstructorInfo constructor)
            {
                // The initial function uses regular reflection.
                _invokeConstructor = cloudEvent =>
                    (CloudEvent)constructor.Invoke(new object[] { cloudEvent });
            }

            public static CopyConstructor? Create(Type type)
            {
                var constructor = GetConstructor(type);

                if (constructor is null)
                {
                    return null;
                }

                return new CopyConstructor(constructor);
            }

            public CloudEvent Invoke(CloudEvent cloudEvent) =>
                _invokeConstructor(cloudEvent);

            private static ConstructorInfo? GetConstructor(Type type) =>
                type.GetConstructor(new[] { type });
        }
    }
}

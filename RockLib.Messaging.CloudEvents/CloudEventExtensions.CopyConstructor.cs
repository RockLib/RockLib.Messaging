using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

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

                // Compile the optimized function in the background.
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var cloudEventParameter = Expression.Parameter(typeof(CloudEvent), "cloudEvent");

                    var body = Expression.New(constructor,
                        Expression.Convert(cloudEventParameter, constructor.DeclaringType));

                    var lamda = Expression.Lambda<Func<CloudEvent, CloudEvent>>(
                        body, cloudEventParameter);

                    // Replace the reflection function with a compiled function.
                    _invokeConstructor = lamda.Compile();
                });
            }

            public static CopyConstructor Create(Type type)
            {
                var constructor = GetConstructor(type);

                if (constructor is null)
                    return null;

                return new CopyConstructor(constructor);
            }

            public CloudEvent Invoke(CloudEvent cloudEvent) =>
                _invokeConstructor(cloudEvent);

            private static ConstructorInfo GetConstructor(Type type) =>
                type.GetConstructor(new[] { type });
        }
    }
}

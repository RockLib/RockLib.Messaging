using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    internal static class Tasks
    {
        static Tasks()
        {
            var source = new TaskCompletionSource<int>();
            source.SetResult(0);
            CompletedTask = source.Task;
        }
        
        public static Task CompletedTask { get; }

        public static Task FromException(Exception exception)
        {
            var source = new TaskCompletionSource<int>();
            source.SetException(exception);
            return source.Task;
        }
    }
}

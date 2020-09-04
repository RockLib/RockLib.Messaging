using RockLib.Reflection.Optimized;
using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    public static class KafkaReceiverExtensions
    {
        public static void Seek(this IReceiver receiver, DateTime timestamp)
        {
            dynamic r = receiver.Undecorate();
            r.Seek(timestamp);
        }

        public static Task Replay(this IReceiver receiver, DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback = null)
        {
            dynamic r = receiver.Undecorate();
            return r.Replay(start, end, callback);
        }
    }
}

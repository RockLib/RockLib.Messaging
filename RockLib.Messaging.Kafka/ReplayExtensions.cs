using RockLib.Reflection.Optimized;
using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    public static class ReplayExtensions
    {
        public static void Seek(this IReceiver receiver, DateTime timestamp)
        {
            dynamic r = receiver.Undecorate();
            r.Seek(timestamp);
        }

        public static void Replay(this IReceiver receiver, DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback = null)
        {
            dynamic r = receiver.Undecorate();
            r.Replay(start, end, callback);
        }
    }
}

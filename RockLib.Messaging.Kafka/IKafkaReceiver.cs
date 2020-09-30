using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    internal interface IKafkaReceiver
    {
        DateTime? StartTimestamp { set; }
        Task ReplayAsync(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback = null, bool pauseDuringReplay = false);
        void Seek(DateTime timestamp);
    }
}
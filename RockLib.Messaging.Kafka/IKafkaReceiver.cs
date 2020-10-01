using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    internal interface IKafkaReceiver
    {
        DateTime? StartTimestamp { set; }
        Task ReplayAsync(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback, bool pauseDuringReplay);
        void Seek(DateTime timestamp);
        void Pause();
        void Resume();
    }
}
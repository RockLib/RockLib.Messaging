using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Kafka
{
    public interface IReplayEngine
    {
        Task Replay(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback, string topic,
            string bootstrapServers, bool enableAutoOffsetStore, AutoOffsetReset autoOffsetReset);
    }
}

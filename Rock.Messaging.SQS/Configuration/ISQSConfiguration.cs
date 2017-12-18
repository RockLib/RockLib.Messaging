#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    public interface ISQSConfiguration
    {
        string Name { get; }
        string QueueUrl { get; }
        int MaxMessages { get; }
        bool AutoAcknowledge { get; }
        bool Compressed { get; }
        bool ParallelHandling { get; }
    }
}
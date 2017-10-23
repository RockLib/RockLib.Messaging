#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    public interface ISQSConfiguration
    {
        string Name { get; set; }
        string QueueUrl { get; set; }
        int MaxMessages { get; set; }
        bool AutoAcknowledge { get; }
        bool Compressed { get; }
        void Validate();
    }
}
namespace Rock.Messaging.SQS
{
    public interface ISQSConfiguration
    {
        string Name { get; set; }
        string QueueUrl { get; set; }
        int MaxMessages { get; set; }
        bool AutoAcknowledge { get; }
        bool Compressed { get; }
    }
}
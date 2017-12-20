namespace Rock.Messaging
{
    public interface IRockMessagingConfiguration
    {
        IMessagingScenarioFactory MessagingScenarioFactory { get; }
    }
}

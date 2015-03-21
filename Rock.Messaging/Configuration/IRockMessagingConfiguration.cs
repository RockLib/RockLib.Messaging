namespace Rock.Messaging.Configuration
{
    public interface IRockMessagingConfiguration
    {
        IMessagingScenarioFactory MessagingScenarioFactory { get; }
    }
}

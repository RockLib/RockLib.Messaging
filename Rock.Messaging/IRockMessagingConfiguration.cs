namespace Rock.Messaging
{
    // TODO: Create implementation of this interface for config.
    public interface IRockMessagingConfiguration
    {
        IMessagingScenarioFactory MessagingScenarioFactory { get; }
    }
}

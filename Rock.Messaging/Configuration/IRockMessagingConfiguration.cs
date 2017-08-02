using Rock.Messaging;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    public interface IRockMessagingConfiguration
    {
        IMessagingScenarioFactory MessagingScenarioFactory { get; }
    }
}

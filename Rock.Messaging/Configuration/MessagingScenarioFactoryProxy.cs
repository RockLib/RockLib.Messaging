using Rock.Serialization;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    public class MessagingScenarioFactoryProxy : XmlDeserializationProxy<IMessagingScenarioFactory>
    {
    }
}
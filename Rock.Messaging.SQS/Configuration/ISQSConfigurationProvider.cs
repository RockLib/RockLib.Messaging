#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    public interface ISQSConfigurationProvider
    {
        ISQSConfiguration GetConfiguration(string name);
        bool HasConfiguration(string name);
    }
}
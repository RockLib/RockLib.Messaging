namespace Rock.Messaging.SQS
{
    public interface ISQSConfigurationProvider
    {
        ISQSConfiguration GetConfiguration(string name);
        bool HasConfiguration(string name);
    }
}
namespace Rock.Messaging.RabbitMQ
{
    public interface IRabbitSessionConfigurationProvider
    {
        IRabbitSessionConfiguration GetConfiguration(string name);
        bool HasConfiguration(string name);
    }
}
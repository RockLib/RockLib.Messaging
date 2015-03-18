namespace Rock.Messaging.NamedPipes
{
    public interface INamedPipeConfigProvider
    {
        INamedPipeConfig GetConfig(string name);
    }
}
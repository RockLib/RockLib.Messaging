namespace Rock.Messaging.NamedPipes
{
    public class SimpleNamedPipeConfigProvider : INamedPipeConfigProvider
    {
        public INamedPipeConfig GetConfig(string name)
        {
            return new SimpleNamedPipeConfig(name);
        }
    }
}
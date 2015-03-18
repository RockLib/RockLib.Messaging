namespace Rock.Messaging.NamedPipes
{
    public class SimpleNamedPipeConfig : INamedPipeConfig
    {
        private readonly string _pipeName;

        public SimpleNamedPipeConfig(string name)
        {
            _pipeName = typeof(NamedPipeMessagingScenarioFactory).Name + "_" + name;
        }

        public string PipeName { get { return _pipeName; } }
    }
}
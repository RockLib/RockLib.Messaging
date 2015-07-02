namespace Rock.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="INamedPipeConfigProvider"/> whose 
    /// <see cref="GetConfig"/> returns instances of <see cref="INamedPipeConfig"/>
    /// with a <see cref="INamedPipeConfig.PipeName"/> property that returns the 
    /// value of the 'name' parameter.
    /// </summary>
    public class SimpleNamedPipeConfigProvider : INamedPipeConfigProvider
    {
        /// <summary>
        /// Gets an implementation of <see cref="INamedPipeConfig"/> whose
        /// <see cref="INamedPipeConfig.PipeName"/> property returns the value of the
        /// <paramref name="name"/> parameter.
        /// </summary>
        /// <param name="name">The name of the named pipe.</param>
        /// <returns>
        /// An implementation of <see cref="INamedPipeConfig"/> whose
        /// <see cref="INamedPipeConfig.PipeName"/> property returns the value of the
        /// <paramref name="name"/> parameter.
        /// </returns>
        public INamedPipeConfig GetConfig(string name)
        {
            return new SimpleNamedPipeConfig(name);
        }

        public bool HasConfig(string name)
        {
            return true;
        }
    }
}
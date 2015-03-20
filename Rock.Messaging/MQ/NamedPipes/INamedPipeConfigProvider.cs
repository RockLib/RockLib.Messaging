namespace Rock.Messaging.NamedPipes
{
    /// <summary>
    /// Provides a mechanism for obtaining instances of <see cref="INamedPipeConfig"/>
    /// via a "name" key.
    /// </summary>
    public interface INamedPipeConfigProvider
    {
        /// <summary>
        /// Gets the configuration for the given name.
        /// </summary>
        /// <param name="name">The name of the config to retrieve.</param>
        /// <returns>The configuration for the given name.</returns>
        INamedPipeConfig GetConfig(string name);
    }
}
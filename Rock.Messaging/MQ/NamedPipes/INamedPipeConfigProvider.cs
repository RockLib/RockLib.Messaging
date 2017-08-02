#if ROCKLIB
namespace RockLib.Messaging.NamedPipes
#else
namespace Rock.Messaging.NamedPipes
#endif
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

        /// <summary>
        /// Returns a value indicating whether an <see cref="INamedPipeConfig"/> can be
        /// retrieved for the given name.
        /// </summary>
        /// <param name="name">The name to test.</param>
        /// <returns>True, if an <see cref="INamedPipeConfig"/> can be retrieved. Otherwise, false.</returns>
        bool HasConfig(string name);
    }
}
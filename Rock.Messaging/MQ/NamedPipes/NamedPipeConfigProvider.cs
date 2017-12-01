using System.Collections.Generic;
using System.Linq;

#if ROCKLIB
namespace RockLib.Messaging.NamedPipes
#else
namespace Rock.Messaging.NamedPipes
#endif
{
    /// <summary>
    /// An blended implementation of <see cref="INamedPipeConfigProvider"/> that
    /// searches configurations in a provided list, or creates new configurations
    /// if no list was provided.
    /// </summary>
    public class NamedPipeConfigProvider : INamedPipeConfigProvider
    {
        private readonly List<NamedPipeConfig> _configs;

        /// <summary>
        /// Initialized a new instance of the <see cref="NamedPipeConfigProvider"/> class.
        /// </summary>
        /// <param name="namedPipeConfigs">A collection of named pipe configurations</param>
        public NamedPipeConfigProvider(List<NamedPipeConfig> namedPipeConfigs = null)
        {
            _configs = namedPipeConfigs;
        }

        /// <summary>
        /// Gets the configuration for the given name, if any configurations were provided.
        /// Otherwise, gets an implementation of <see cref="INamedPipeConfig"/> whose
        /// <see cref="INamedPipeConfig.PipeName"/> property returns the value of the
        /// <paramref name="name"/> parameter.
        /// </summary>
        /// <param name="name">The name of the config to retrieve.</param>
        /// <returns>
        /// The configuration for the given name, if any configurations were provided.
        /// Otherwise, returns an implementation of <see cref="INamedPipeConfig"/> whose
        /// <see cref="INamedPipeConfig.PipeName"/> property returns the value of the
        /// <paramref name="name"/> parameter.
        /// </returns>
        public INamedPipeConfig GetConfig(string name)
        {
            return _configs?.First(c => c.Name == name) ?? new NamedPipeConfig(name, name, false);
        }

        /// <summary>
        /// Returns a value indicating whether an <see cref="INamedPipeConfig"/> can be
        /// retrieved for the given name, if any configurations were provided.
        /// Otherwise, always returns true.
        /// </summary>
        /// <param name="name">The name to test.</param>
        /// <returns>True, if an <see cref="INamedPipeConfig"/> can be retrieved, or no
        /// configurations were provided. Otherwise, false.</returns>
        public bool HasConfig(string name)
        {
            return _configs?.Any(c => c.Name == name) ?? true;
        }
    }
}
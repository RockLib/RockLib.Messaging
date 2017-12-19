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
        private readonly IReadOnlyDictionary<string, NamedPipeConfig> _configs;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeConfigProvider"/> class.
        /// If <paramref name="namedPipeConfigs"/> is null, this instance will be configured
        /// for "simple mode": where the name value passed to <see cref="GetConfig(string)"/>
        /// is used for the pipe name, and <see cref="HasConfig(string)"/> always returns true.
        /// </summary>
        /// <param name="namedPipeConfigs">
        /// A collection of named pipe configurations, or null if this instance.
        /// </param>
        public NamedPipeConfigProvider(IEnumerable<NamedPipeConfig> namedPipeConfigs = null)
        {
            _configs = namedPipeConfigs?.ToDictionary(c => c.Name);
        }

        /// <summary>
        /// Gets the collection of <see cref="NamedPipeConfig"/> objects that represent the
        /// backing store for this instance. Returns null if this instance is configured for
        /// "simple mode": where the name value passed to <see cref="GetConfig(string)"/> is
        /// used for the pipe name, and <see cref="HasConfig(string)"/> always returns true.
        /// </summary>
        public IEnumerable<NamedPipeConfig> Configurations { get { return _configs?.Values; } }

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
            return _configs?[name] ?? new NamedPipeConfig(name);
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
            return _configs?.ContainsKey(name) ?? true;
        }
    }
}
using System;

#if ROCKLIB
namespace RockLib.Messaging.NamedPipes
#else
namespace Rock.Messaging.NamedPipes
#endif
{
    /// <summary>
    /// A simple implementation of <see cref="INamedPipeConfig"/>.
    /// </summary>
    public class NamedPipeConfig : INamedPipeConfig
    {
        private readonly string _name;
        private readonly string _pipeName;
        private readonly bool _compressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeConfig"/> class.
        /// </summary>
        /// <param name="name">The configuration name.</param>
        /// <param name="pipeName">The name of the named pipe.</param>
        /// <param name="compressed">Whether messages should be compressed.</param>
        public NamedPipeConfig(string name, string pipeName = null, bool compressed = false)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _name = name;
            _pipeName = pipeName ?? name;
            _compressed = compressed;
        }

        /// <summary>
        /// Gets the name of the named pipe.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets the name of the named pipe.
        /// </summary>
        public string PipeName { get { return _pipeName; } }

        /// <summary>
        /// Gets whether messages should be compressed.
        /// </summary>
        public bool Compressed { get { return _compressed; } }
    }
}
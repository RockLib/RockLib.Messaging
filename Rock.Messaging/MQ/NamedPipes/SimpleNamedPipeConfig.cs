#if ROCKLIB
namespace RockLib.Messaging.NamedPipes
#else
namespace Rock.Messaging.NamedPipes
#endif
{
    /// <summary>
    /// A simple implementation of <see cref="INamedPipeConfig"/>.
    /// </summary>
    public class SimpleNamedPipeConfig : INamedPipeConfig
    {
        private readonly string _pipeName;
        private readonly bool _compressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleNamedPipeConfig"/> class.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe.</param>
        /// <param name="compressed">Whether messages should be compressed.</param>
        public SimpleNamedPipeConfig(string pipeName, bool compressed)
        {
            _pipeName = pipeName;
            _compressed = compressed;
        }

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
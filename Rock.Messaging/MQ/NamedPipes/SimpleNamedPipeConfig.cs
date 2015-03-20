namespace Rock.Messaging.NamedPipes
{
    /// <summary>
    /// A simple implementation of <see cref="INamedPipeConfig"/>.
    /// The value passed to the constructor is returned by the
    /// <see cref="PipeName"/> property.
    /// </summary>
    public class SimpleNamedPipeConfig : INamedPipeConfig
    {
        private readonly string _pipeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleNamedPipeConfig"/> class.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe.</param>
        public SimpleNamedPipeConfig(string pipeName)
        {
            _pipeName = pipeName;
        }

        /// <summary>
        /// Gets the name of the named pipe.
        /// </summary>
        public string PipeName { get { return _pipeName; } }
    }
}
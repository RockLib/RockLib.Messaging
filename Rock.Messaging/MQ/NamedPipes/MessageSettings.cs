namespace RockLib.Messaging.MQ.NamedPipes
{
    /// <summary>
    /// Defines the settings for a named pipe connection.
    /// </summary>
    public class MessagingSettings
    {
        /// <summary>
        /// Gets or sets the name of the named pipe.
        /// </summary>
        public string PipeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the messages sent through this connection should be compressed.
        /// </summary>
        public bool Compressed { get; set; }
    }
}

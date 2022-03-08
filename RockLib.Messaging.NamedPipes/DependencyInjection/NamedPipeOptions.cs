using RockLib.Messaging.NamedPipes;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="NamedPipeSender"/> and
    /// <see cref="NamedPipeReceiver"/>.
    /// </summary>
    public class NamedPipeOptions
    {
        /// <summary>
        /// Gets or sets the pipe name. If null, the name of the sender or receiver is used as
        /// the pipe name.
        /// </summary>
        public string? PipeName { get; set; }
    }
}
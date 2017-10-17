using System.Collections.Generic;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    /// <summary>
    /// Defines the interface for an outgoing message.
    /// </summary>
    public interface ISenderMessage
    {
        /// <summary>
        /// Gets the string value of the message.
        /// </summary>
        string StringValue { get; }

        /// <summary>
        /// Gets the binary value of the message.
        /// </summary>
        byte[] BinaryValue { get; }

        /// <summary>
        /// Gets the message format of the message.
        /// </summary>
        MessageFormat MessageFormat { get; }

        /// <summary>
        /// Gets the headers of the message.
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> Headers { get; }

        /// <summary>
        /// Gets the priority of the message.
        /// </summary>
        byte? Priority { get; }

        /// <summary>
        /// Gets a value indicating whether the message should be compressed when sending.
        /// If null, compression is determined by the sender's configuration.
        /// </summary>
        bool? Compressed { get; }
    }
}
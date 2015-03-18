using System.Collections.Generic;

namespace Rock.Messaging
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
    }
}
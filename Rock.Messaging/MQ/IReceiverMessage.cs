using System.Text;

namespace Rock.Messaging
{
    /// <summary>
    /// Defines the interface for a received message.
    /// </summary>
    public interface IReceiverMessage
    {
        /// <summary>
        /// Gets the string value of the message. If the implemenation "speaks" binary,
        /// <paramref name="encoding"/> is used to convert the binary message to a string.
        /// If <paramref name="encoding"/> is null, the binary data will be converted using
        /// base 64 encoding.
        /// </summary>
        /// <param name="encoding">
        /// The encoding to use. A null value indicates that base 64 encoding should be used.
        /// </param>
        /// <returns>The string value of the message.</returns>
        string GetStringValue(Encoding encoding);

        /// <summary>
        /// Gets the binary value of the message. If the implemenation "speaks" string,
        /// <paramref name="encoding"/> is used to convert the string message to a byte array.
        /// If <paramref name="encoding"/> is null, the string data will be converted using
        /// base 64 encoding.
        /// </summary>
        /// <param name="encoding">
        /// The encoding to use. A null value indicates that base 64 encoding should be used.
        /// </param>
        /// <returns>The binary value of the message.</returns>
        byte[] GetBinaryValue(Encoding encoding);

        /// <summary>
        /// Gets a header value by key. If the implementation "speaks" binary,
        /// <paramref name="encoding"/> is used to convert the binary header to a string.
        /// If <paramref name="encoding"/> is null, the binary header will be converted
        /// using base 64 encoding.
        /// </summary>
        /// <param name="key">The key of the header to retrieve.</param>
        /// <param name="encoding">
        /// The encoding to use. A null value indicates that base 64 encoding should be used.
        /// </param>
        /// <returns>The string value of the header.</returns>
        string GetHeaderValue(string key, Encoding encoding);

        /// <summary>
        /// Acknowledges the message.
        /// </summary>
        void Acknowledge();
    }
}
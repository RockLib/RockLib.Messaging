using System.Text;

namespace Rock.Messaging
{
    /// <summary>
    /// Provides a set of methods for obtaining values from objects that implement
    /// <see cref="IReceiverMessage"/>.
    /// </summary>
    public static class ReceiverMessageExtensions
    {
        /// <summary>
        /// Gets the string value of the message. If the implemenation "speaks" binary,
        /// <see cref="Encoding.UTF8"/> is used to convert the binary message to a string.
        /// </summary>
        /// <param name="source">The message from which to obtain a string value.</param>
        /// <returns>The string value of the message.</returns>
        public static string GetStringValue(this IReceiverMessage source)
        {
            return source.GetStringValue(Encoding.UTF8);
        }

        /// <summary>
        /// Gets the binary value of the message. If the implemenation "speaks" string,
        /// <see cref="Encoding.UTF8"/> is used to convert the string message to a byte array.
        /// </summary>
        /// <param name="source">The message from which to obtain a binary value.</param>
        /// <returns>The binary value of the message.</returns>
        public static byte[] GetBinaryValue(this IReceiverMessage source)
        {
            return source.GetBinaryValue(Encoding.UTF8);
        }

        /// <summary>
        /// Gets a header value by key. If the implementation "speaks" binary,
        /// <see cref="Encoding.UTF8"/> is used to convert the binary header to a string.
        /// </summary>
        /// <param name="source">The message from which to obtain a binary value.</param>
        /// <param name="key">The key of the header to retrieve.</param>
        /// <returns>The string value of the header.</returns>
        public static string GetHeaderValue(this IReceiverMessage source, string key)
        {
            return source.GetHeaderValue(key, Encoding.UTF8);
        }
    }
}
using System.Text;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    /// <summary>
    /// Provides compression and decompression methods for use by queues.
    /// </summary>
    public static class MessageCompression
    {
        /// <summary>
        /// Compress the string value of a message.
        /// </summary>
        /// <param name="value">A string to compress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array. If null,
        /// <see cref="Encoding.UTF8"/> is used.
        /// </param>
        /// <returns>The compressed string.</returns>
        public static string Compress(string value, Encoding encoding = null)
        {
            return DefaultMessageCompressor.Current.Compress(value, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// Compress the byte array value of a message.
        /// </summary>
        /// <param name="value">A byte array to compress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array. If null,
        /// <see cref="Encoding.UTF8"/> is used.
        /// </param>
        /// <returns>The compressed byte array.</returns>
        public static byte[] Compress(byte[] value, Encoding encoding = null)
        {
            return DefaultMessageCompressor.Current.Compress(value, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// Decompress the string value of a message.
        /// </summary>
        /// <param name="value">A string to decompress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array. If null,
        /// <see cref="Encoding.UTF8"/> is used.
        /// </param>
        /// <returns>The decompressed string.</returns>
        public static string Decompress(string value, Encoding encoding = null)
        {
            return DefaultMessageCompressor.Current.Decompress(value, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// Decompress the byte array value of a message.
        /// </summary>
        /// <param name="value">A byte array to decompress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array. If null,
        /// <see cref="Encoding.UTF8"/> is used.
        /// </param>
        /// <returns>The decompressed byte array.</returns>
        public static byte[] Decompress(byte[] value, Encoding encoding = null)
        {
            return DefaultMessageCompressor.Current.Decompress(value, encoding ?? Encoding.UTF8);
        }
    }
}
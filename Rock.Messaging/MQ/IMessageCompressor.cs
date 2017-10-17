using System.Text;

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    /// <summary>
    /// Defines an interface for compressing and decompressing message payloads.
    /// </summary>
    public interface IMessageCompressor
    {
        /// <summary>
        /// Compress the string value of a message.
        /// </summary>
        /// <param name="value">A string to compress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array.
        /// </param>
        /// <returns>The compressed string.</returns>
        string Compress(string value, Encoding encoding);

        /// <summary>
        /// Compress the byte array value of a message.
        /// </summary>
        /// <param name="value">A byte array to compress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array.
        /// </param>
        /// <returns>The compressed byte array.</returns>
        byte[] Compress(byte[] value, Encoding encoding);

        /// <summary>
        /// Decompress the string value of a message.
        /// </summary>
        /// <param name="value">A string to decompress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array.
        /// </param>
        /// <returns>The decompressed string.</returns>
        string Decompress(string value, Encoding encoding);

        /// <summary>
        /// Decompress the byte array value of a message.
        /// </summary>
        /// <param name="value">A byte array to decompress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array.
        /// </param>
        /// <returns>The decompressed byte array.</returns>
        byte[] Decompress(byte[] value, Encoding encoding);
    }
}
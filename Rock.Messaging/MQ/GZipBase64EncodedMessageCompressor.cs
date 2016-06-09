using System;
using System.Text;
using Rock.Compression;

namespace Rock.Messaging
{
    /// <summary>
    /// An implementation of <see cref="IMessageCompressor"/> that uses GZip for compression
    /// and base-64 encoded the resulting
    /// </summary>
    public class GZipBase64EncodedMessageCompressor : IMessageCompressor
    {
        /// <summary>
        /// Compress the string value of a message.
        /// </summary>
        /// <param name="value">A string to compress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array.
        /// </param>
        /// <returns>The compressed string.</returns>
        public string Compress(string value, Encoding encoding)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (encoding == null) throw new ArgumentNullException("encoding");

            var compressor = new GZipCompressor();

            return Convert.ToBase64String(compressor.Compress(encoding.GetBytes(value)));
        }

        /// <summary>
        /// Compress the byte array value of a message.
        /// </summary>
        /// <param name="value">A byte array to compress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array.
        /// </param>
        /// <returns>The compressed byte array.</returns>
        public byte[] Compress(byte[] value, Encoding encoding)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (encoding == null) throw new ArgumentNullException("encoding");

            var compressor = new GZipCompressor();

            return encoding.GetBytes(Convert.ToBase64String(compressor.Compress(value)));
        }

        /// <summary>
        /// Decompress the string value of a message.
        /// </summary>
        /// <param name="value">A string to decompress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array.
        /// </param>
        /// <returns>The decompressed string.</returns>
        public string Decompress(string value, Encoding encoding)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (encoding == null) throw new ArgumentNullException("encoding");

            var decompressor = new GZipDecompressor();

            return encoding.GetString(decompressor.Decompress(Convert.FromBase64String(value)));
        }

        /// <summary>
        /// Decompress the byte array value of a message.
        /// </summary>
        /// <param name="value">A byte array to decompress.</param>
        /// <param name="encoding">
        /// The encoding used to convert the string to a byte array.
        /// </param>
        /// <returns>The decompressed byte array.</returns>
        public byte[] Decompress(byte[] value, Encoding encoding)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (encoding == null) throw new ArgumentNullException("encoding");

            var decompressor = new GZipDecompressor();

            return decompressor.Decompress(Convert.FromBase64String(encoding.GetString(value)));
        }
    }
}
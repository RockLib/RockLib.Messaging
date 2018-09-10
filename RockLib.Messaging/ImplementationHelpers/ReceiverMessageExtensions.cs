using RockLib.Compression;
using System;
using System.Text;

namespace RockLib.Messaging.ImplementationHelpers
{
    /// <summary>
    /// Provides extension methods for implementations of <see cref="IReceiverMessage"/>
    /// to use. These extension methods simplify the process of decompressing and decoding
    /// raw string and binary values.
    /// </summary>
    public static class ReceiverMessageExtensions
    {
        private static readonly GZipDecompressor _gzip = new GZipDecompressor();

        /// <summary>
        /// Sets the <paramref name="_stringPayload"/> and <paramref name="_binaryPayload"/>
        /// parameters based on the specified raw string payload.
        /// <para>
        /// NOTE: This method is intended to be invoked from an <see cref="IReceiverMessage"/>
        /// implementation's constructor, AFTER its <see cref="IReceiverMessage.Headers"/>
        /// property has been initialized and loaded with the headers of the incoming message.
        /// It is assumed that this implementation has private readonly fields that are set
        /// by the out parameters of this method, and that the implementations of the
        /// <see cref="IReceiverMessage.StringPayload"/> and <see cref="IReceiverMessage.BinaryPayload"/>
        /// return the values of these fields.
        /// </para>
        /// </summary>
        /// <param name="receiverMessage">
        /// A message whose IsCompressed and IsBinary headers (or lack thereof) define the compression
        /// and encoding of the raw payload.
        /// </param>
        /// <param name="rawStringPayload">The raw string payload.</param>
        /// <param name="_stringPayload">
        /// When this method returns, a lazy string that contains the uncompressed string payload.
        /// </param>
        /// <param name="_binaryPayload">
        /// When this method returns, a lazy byte array that contains the uncompressed binary payload.
        /// </param>
        public static void SetLazyPayloadFields(this IReceiverMessage receiverMessage, string rawStringPayload, out Lazy<string> _stringPayload, out Lazy<byte[]> _binaryPayload)
        {
            if (receiverMessage.IsCompressed())
            {
                _binaryPayload = new Lazy<byte[]>(() => _gzip.Decompress(Convert.FromBase64String(rawStringPayload)));
                receiverMessage.SetStringPayloadFieldFromBinary(_binaryPayload, out _stringPayload);
            }
            else
            {
                _stringPayload = new Lazy<string>(() => rawStringPayload);

                if (receiverMessage.IsBinary())
                    _binaryPayload = new Lazy<byte[]>(() => Convert.FromBase64String(rawStringPayload));
                else
                    _binaryPayload = new Lazy<byte[]>(() => Encoding.UTF8.GetBytes(rawStringPayload));
            }
        }

        /// <summary>
        /// Sets the <paramref name="_stringPayload"/> and <paramref name="_binaryPayload"/>
        /// parameters based on the specified raw binary payload.
        /// <para>
        /// NOTE: This method is intended to be invoked from an <see cref="IReceiverMessage"/>
        /// implementation's constructor, AFTER its <see cref="IReceiverMessage.Headers"/>
        /// property has been initialized and loaded with the headers of the incoming message.
        /// It is assumed that this implementation has private readonly fields that are set
        /// by the out parameters of this method, and that the implementations of the
        /// <see cref="IReceiverMessage.StringPayload"/> and <see cref="IReceiverMessage.BinaryPayload"/>
        /// return the values of these fields.
        /// </para>
        /// </summary>
        /// <param name="receiverMessage">
        /// A message whose IsCompressed and IsBinary headers (or lack thereof) define the compression
        /// and encoding of the raw payload.
        /// </param>
        /// <param name="rawBinaryPayload">The raw binary payload.</param>
        /// <param name="_stringPayload">
        /// When this method returns, a lazy string that contains the uncompressed string payload.
        /// </param>
        /// <param name="_binaryPayload">
        /// When this method returns, a lazy byte array that contains the uncompressed binary payload.
        /// </param>
        public static void SetLazyPayloadFields(this IReceiverMessage receiverMessage, byte[] rawBinaryPayload, out Lazy<string> _stringPayload, out Lazy<byte[]> _binaryPayload)
        {
            if (receiverMessage.IsCompressed())
                _binaryPayload = new Lazy<byte[]>(() => _gzip.Decompress(rawBinaryPayload));
            else
                _binaryPayload = new Lazy<byte[]>(() => rawBinaryPayload);

            receiverMessage.SetStringPayloadFieldFromBinary(_binaryPayload, out _stringPayload);
        }

        private static void SetStringPayloadFieldFromBinary(this IReceiverMessage receiverMessage, Lazy<byte[]> binaryPayload, out Lazy<string> _stringPayload)
        {
            if (receiverMessage.IsBinary())
                _stringPayload = new Lazy<string>(() => Convert.ToBase64String(binaryPayload.Value));
            else
                _stringPayload = new Lazy<string>(() => Encoding.UTF8.GetString(binaryPayload.Value));
        }
    }
}

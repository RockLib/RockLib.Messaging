using RockLib.Compression;
using System;
using System.Text;

namespace RockLib.Messaging.ImplementationHelpers
{
    public static class ReceiverMessageExtensions
    {
        private static readonly GZipDecompressor _gzip = new GZipDecompressor();

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

using System.IO.Compression;
using System.IO;

namespace RockLib.Messaging;

// Lifted from:
// https://github.com/RockLib/RockLib.Compression/blob/main/RockLib.Compression/GZipCompressor.cs
// https://github.com/RockLib/RockLib.Compression/blob/main/RockLib.Compression/CompressionExtensions.cs
internal static class GZipCompressor
{
    internal static byte[] Compress(byte[] data)
    {
        using var inputStream = new MemoryStream(data);
        using var outputStream = new MemoryStream();
        using (var gzStream = new GZipStream(outputStream, CompressionMode.Compress, true))
        {
            inputStream.CopyTo(gzStream);
        }

        return outputStream.ToArray();
    }
}

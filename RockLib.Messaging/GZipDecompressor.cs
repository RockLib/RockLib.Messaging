using System;
using System.IO;
using System.IO.Compression;

namespace RockLib.Messaging;

// Lifted from:
// https://github.com/RockLib/RockLib.Compression/blob/main/RockLib.Compression/GZipDecompressor.cs
// https://github.com/RockLib/RockLib.Compression/blob/main/RockLib.Compression/CompressionExtensions.cs
internal static class GZipDecompressor
{
    internal static byte[] Decompress(byte[] data)
    {
        using var inputStream = new MemoryStream(data);
        using var outputStream = new MemoryStream();
        using (var gzStream = new GZipStream(inputStream, CompressionMode.Decompress, true))
        {
            gzStream.CopyTo(outputStream);
        }

        return outputStream.ToArray();
    }
}

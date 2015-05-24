using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gelf.Tests
{
    public static class TestUtils
    {
        public static string DecompressGzipMessage(byte[] data, Encoding encoding)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                var decompressedData = resultStream.ToArray();
                return encoding.GetString(decompressedData);
            }
        }
    }
}
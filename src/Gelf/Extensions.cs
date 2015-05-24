using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gelf
{
    internal static class Extensions
    {
        /// <summary>
        /// Gzips a string
        /// </summary>
        public static byte[] GzipMessage(this string message, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(message);
            
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static double ToUnixTimestamp(this DateTime d)
        {
            var duration = d.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return duration.TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(this double d)
        {
            var datetime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(d);
            return datetime;
        }
    }
}
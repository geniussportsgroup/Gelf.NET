using System.Text;

namespace Gelf
{
    internal static class Constants
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
        public const int MaxHeaderSize = 8;
        public const int MaxChunkSize = 1024;
    }
}
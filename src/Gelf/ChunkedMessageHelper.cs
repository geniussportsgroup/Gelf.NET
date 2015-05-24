using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Gelf
{
    internal static class ChunkedMessageHelper
    {
        public static byte[] GetMessageChunkFull(byte[] bytes, string messageId, int i, int chunkCount)
        {
            var messageChunkPrefix = CreateChunkedMessagePart(messageId, i, chunkCount);
            var skip = i * Constants.MaxChunkSize;
            var messageChunkSuffix = bytes.Skip(skip).Take(Constants.MaxChunkSize).ToArray();

            var messageChunkFull = new byte[messageChunkPrefix.Length + messageChunkSuffix.Length];
            messageChunkPrefix.CopyTo(messageChunkFull, 0);
            messageChunkSuffix.CopyTo(messageChunkFull, messageChunkPrefix.Length);

            return messageChunkFull;
        }

        public static string GenerateMessageId()
        {
            var md5String = String.Join("", MD5.Create().ComputeHash(Encoding.Default.GetBytes(Environment.MachineName)).Select(it => it.ToString("x2")).ToArray());
            var random = new Random((int)DateTime.Now.Ticks);
            var sb = new StringBuilder();
            var t = DateTime.Now.Ticks % 1000000000;
            var s = String.Format("{0}{1}", md5String.Substring(0, 10), md5String.Substring(20, 10));
            var r = random.Next(10000000).ToString("00000000");

            sb.Append(t);
            sb.Append(s);
            sb.Append(r);

            //Message ID: 8 bytes 
            return sb.ToString().Substring(0, Constants.MaxHeaderSize);
        }

        public static byte[] CreateChunkedMessagePart(string messageId, int index, int chunkCount)
        {
            var result = new List<byte>();
            var gelfHeader = new[] { Convert.ToByte(30), Convert.ToByte(15) };
            result.AddRange(gelfHeader);
            result.AddRange(Encoding.Default.GetBytes(messageId).ToArray());
            result.Add(Convert.ToByte(index));
            result.Add(Convert.ToByte(chunkCount));

            return result.ToArray<byte>();
        }
    }
}

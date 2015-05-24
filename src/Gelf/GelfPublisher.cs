using System;
using System.Diagnostics;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Gelf
{
    /// <summary>
    /// UDP publisher to send Gelf messages to a Graylog server
    /// </summary>
    public class GelfPublisher : IGelfPublisher
    {
        /// <summary>
        /// Gets Graylog Server Hostname.
        /// </summary>
        public string RemoteHostname { get; private set; }

        /// <summary>
        /// Gets Graylog Server Host Port.
        /// </summary>
        public int RemoteHostPort { get; private set; }

        private readonly UdpClient client;

        public GelfPublisher(string remoteHostname, int remoteHostPort)
        {
            RemoteHostname = remoteHostname;
            RemoteHostPort = remoteHostPort;
            
            client = new UdpClient(RemoteHostname, RemoteHostPort);
        }

        /// <summary>
        /// Publishes a Gelf message to the specified Graylog server.
        /// </summary>
        public void Publish(GelfMessage message)
        {
            var serializedGelfMessage = JsonConvert.SerializeObject(message, Formatting.Indented);
            byte[] bytes = serializedGelfMessage.GzipMessage(Constants.Encoding);

            if (Constants.MaxChunkSize < bytes.Length)
            {
                var chunkCount = (bytes.Length / Constants.MaxChunkSize) + 1;
                var messageId = ChunkedMessageHelper.GenerateMessageId();
                var state = new UdpState { SendClient = client, Bytes = bytes, ChunkCount = chunkCount, MessageId = messageId, SendIndex = 0 };
                var messageChunkFull = ChunkedMessageHelper.GetMessageChunkFull(state.Bytes, state.MessageId, state.SendIndex, state.ChunkCount);
                client.BeginSend(messageChunkFull, messageChunkFull.Length, SendCallback, state);
            }
            else
            {
                var state = new UdpState { SendClient = client, Bytes = bytes, ChunkCount = 0, MessageId = string.Empty, SendIndex = 0 };
                client.BeginSend(bytes, bytes.Length, SendCallback, state);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var state = (UdpState)ar.AsyncState;
            var u = state.SendClient;

            var bytesSent = u.EndSend(ar);
            Debug.WriteLine("Async bytes sent {0} chunk {1} of {2}", bytesSent, state.SendIndex, state.ChunkCount);

            state.SendIndex++;
            if (state.SendIndex < state.ChunkCount)
            {
                var messageChunkFull = ChunkedMessageHelper.GetMessageChunkFull(state.Bytes, state.MessageId, state.SendIndex, state.ChunkCount);
                state.SendClient.BeginSend(messageChunkFull, messageChunkFull.Length, SendCallback, state);
            }
        }

        private class UdpState
        {
            public UdpClient SendClient { set; get; }
            public int ChunkCount { set; get; }
            public string MessageId { set; get; }
            public int SendIndex { set; get; }
            public byte[] Bytes { set; get; }
        }
    }
}

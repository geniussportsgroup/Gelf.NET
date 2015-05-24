using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Gelf.Tests
{
    [TestFixture]
    public class GelfPublisherTest
    {
        private const string Hostname = "localhost";
        private const int Port = 12201;

        private GelfPublisher gelfPublisher;

        [SetUp]
        public void SetUp()
        {
            gelfPublisher = new GelfPublisher(Hostname, Port);
        }

        [Test]
        public void IntegrationTest()
        {
            // Arrange
            var receiveEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
            var manualResetEvent = new ManualResetEvent(false);
            var gelfMessageToSend = new GelfMessage
            {
                Host = Environment.MachineName,
                FullMessage = "Full message",
                ShortMessage = "Short message",
            };
            gelfMessageToSend["correlationId"] = Guid.NewGuid().ToString();

            string receivedMessage = null;
            bool wasMessageReceived;

            using (var udp = new UdpClient(Port, AddressFamily.InterNetworkV6))
            {
                var localUdpClient = udp; // Avoid Resharper access to Disposed closure warning
                localUdpClient.BeginReceive(ar =>
                {
                    var receiveBytes = localUdpClient.EndReceive(ar, ref receiveEndPoint);
                    receivedMessage = TestUtils.DecompressGzipMessage(receiveBytes, Constants.Encoding);
                    manualResetEvent.Set();
                }, null);

                // Act
                gelfPublisher.Publish(gelfMessageToSend);

                wasMessageReceived = manualResetEvent.WaitOne(TimeSpan.FromSeconds(2));
            }

            // Assert
            Assert.That(wasMessageReceived, Is.True, "Message not received in time");

            dynamic receivedObject = JObject.Parse(receivedMessage);
            Assert.That(receivedObject.correlationId.ToString(), Is.EqualTo(gelfMessageToSend["correlationId"]));
            Assert.That(receivedObject.host.ToString(), Is.EqualTo(gelfMessageToSend.Host));
            Assert.That(receivedObject.full_message.ToString(), Is.EqualTo(gelfMessageToSend.FullMessage));
            Assert.That(receivedObject.short_message.ToString(), Is.EqualTo(gelfMessageToSend.ShortMessage));
        }
    }
}
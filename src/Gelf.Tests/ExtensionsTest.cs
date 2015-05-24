using System;
using System.Text;
using NUnit.Framework;

namespace Gelf.Tests
{
    [TestFixture]
    public class ExtensionsTest
    {
        [Test]
        public void ToUnixTimestamp()
        {
            // Arrange
            var utcNow = DateTime.UtcNow;
            var expectedResult = utcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            // Act
            var result = utcNow.ToUnixTimestamp();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void FromUnixTimestamp()
        {
            // Arrange
            var utcNow = DateTime.UtcNow;
            var input = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, DateTimeKind.Utc);
            var timestamp = input.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            // Act
            var result = timestamp.FromUnixTimestamp();

            // Assert
            Assert.That(result, Is.EqualTo(input));
        }
        
        [Test]
        public void GzipMessage()
        {
            // Arrange
            var message = "I'm a test message";
            var encoding = Encoding.UTF8;

            // Act
            var compressedMessage = message.GzipMessage(encoding);

            // Act
            var decompressGzipMessage = TestUtils.DecompressGzipMessage(compressedMessage, encoding);
            Assert.That(decompressGzipMessage, Is.EqualTo(message));
        }
    }
}

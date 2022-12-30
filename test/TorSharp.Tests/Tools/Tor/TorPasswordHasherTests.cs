using System;
using System.Linq;
using Knapcode.TorSharp.Adapters;
using Knapcode.TorSharp.Tests.TestSupport;
using Knapcode.TorSharp.Tools.Tor;
using Moq;
using Xunit;

namespace Knapcode.TorSharp.Tests.Tools.Tor
{
    public class TorPasswordHasherTests
    {
        [Fact]
        [DisplayTestMethodName]
        public void TorPasswordHasher_MatchesKnownInput()
        {
            // Arrange
            var password = "foobar";
            var expected = "16:F5B2CC984516D0C360326BB33D50A4D75F2496D27ED572A7A5C6676648";

            // Act & Assert
            MatchesExpectedHashedPassword(password, expected);
        }

        private void MatchesExpectedHashedPassword(string password, string expected)
        {
            // Arrange
            var salt = GetSalt(expected);

            var random = new Mock<IRandom>();
            random
                .Setup(x => x.GetBytes(It.IsAny<byte[]>()))
                .Callback<byte[]>(x => Buffer.BlockCopy(salt, 0, x, 0, Math.Min(salt.Length, x.Length)));

            var randomFactory = new Mock<IRandomFactory>();
            randomFactory
                .Setup(x => x.Create())
                .Returns(random.Object);

            var torPasswordHasher = new TorPasswordHasher(randomFactory.Object);

            // Act
            var actual = torPasswordHasher.HashPassword(password);

            // Assert
            Assert.Equal(expected, actual);
        }

        private byte[] GetSalt(string torPassword)
        {
            var salt = torPassword.Substring(3, 16);
            return HexToBytes(salt);
        }

        private byte[] HexToBytes(string hex)
        {
            return Enumerable
                .Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}

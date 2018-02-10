using Xunit;

namespace Knapcode.TorSharp.Tests
{
    public class TorSharpPrivoxySettingsTests
    {
        [Fact]
        public void HasExpectedDefaultSocksPort()
        {
            Assert.Equal(18118, new TorSharpPrivoxySettings().Port);
        }
    }
}

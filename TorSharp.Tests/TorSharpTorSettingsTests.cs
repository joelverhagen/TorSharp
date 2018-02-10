using Xunit;

namespace Knapcode.TorSharp.Tests
{
    public class TorSharpTorSettingsTests
    {
        [Fact]
        public void HasExpectedDefaultSocksPort()
        {
            Assert.Equal(19050, new TorSharpTorSettings().SocksPort);
        }

        [Fact]
        public void HasExpectedDefaultControlPort()
        {
            Assert.Equal(19051, new TorSharpTorSettings().ControlPort);
        }
    }
}

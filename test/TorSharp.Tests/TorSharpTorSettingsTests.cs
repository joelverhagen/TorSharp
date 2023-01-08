using Knapcode.TorSharp.Tests.TestSupport;
using Xunit;

namespace Knapcode.TorSharp.Tests
{
    public class TorSharpTorSettingsTests
    {
        [Fact]
        [DisplayTestMethodName]
        public void HasExpectedDefaultSocksPort()
        {
            Assert.Equal(19050, new TorSharpTorSettings().SocksPort);
        }

        [Fact]
        [DisplayTestMethodName]
        public void HasExpectedDefaultControlPort()
        {
            Assert.Equal(19051, new TorSharpTorSettings().ControlPort);
        }

        [Fact]
        [DisplayTestMethodName]
        public void HasDefaultControlPassword()
        {
            var settings = new TorSharpTorSettings();

            Assert.NotNull(settings.ControlPassword);
            Assert.Null(settings.HashedControlPassword);
            Assert.Equal(TorSharpTorSettings.DefaultControlPassword, settings.ControlPassword);
        }
    }
}

using Knapcode.TorSharp.Tests.TestSupport;
using Xunit;

namespace Knapcode.TorSharp.Tests
{
    public class TorSharpPrivoxySettingsTests
    {
        [Fact]
        [DisplayTestMethodName]
        public void HasExpectedDefaultSocksPort()
        {
            Assert.Equal(18118, new TorSharpPrivoxySettings().Port);
        }
    }
}

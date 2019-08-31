using System.Threading.Tasks;
using Knapcode.TorSharp.Tests.TestSupport;
using Knapcode.TorSharp.Tools.Tor;
using Xunit;

namespace Knapcode.TorSharp.Tests.Tools.Tor
{
    public class TorControlClientTests
    {
        [Fact]
        [DisplayTestMethodName]
        public async Task CanDisposeAfterFailedConnect()
        {
            TorControlClient client = null;
            try
            {
                client = new TorControlClient();
                await client.ConnectAsync("300.0.0.0", 12345);
            }
            catch
            {
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}

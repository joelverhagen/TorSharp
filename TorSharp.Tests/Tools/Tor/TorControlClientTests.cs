using System.Threading.Tasks;
using Knapcode.TorSharp.Tools.Tor;
using Xunit;

namespace Knapcode.TorSharp.Tests.Tools.Tor
{
    public class TorControlClientTests
    {
        [Fact]
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

using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tests.TestSupport;
using Xunit;

namespace Knapcode.TorSharp.Tests
{
    public class EndToEnd
    {
        [Fact]
        public async Task EndToEnd_VirtualDesktopToolRunner()
        {
            using (var te = TestEnvironment.Initialize())
            {
                // Arrange
                var settings = te.BuildSettings();
                settings.ToolRunnerType = ToolRunnerType.VirtualDesktop;

                // Act & Assert
                await ExecuteEndToEndTestAsync(settings);
            }
        }

        [Fact]
        public async Task EndToEnd_SimpleToolRunner()
        {
            using (var te = TestEnvironment.Initialize())
            {
                // Arrange
                var settings = te.BuildSettings();
                settings.ToolRunnerType = ToolRunnerType.Simple;

                // Act & Assert
                await ExecuteEndToEndTestAsync(settings);
            }
        }

        private static async Task ExecuteEndToEndTestAsync(TorSharpSettings settings)
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxyPort))
            };

            using (var httpClient = new HttpClient(handler))
            using (var proxy = new TorSharpProxy(settings))
            {
                // Act
                // download tools
                await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();

                // start the proxy
                await proxy.ConfigureAndStartAsync();

                // execute some HTTP requests
                var unparsedIpA = await httpClient.GetStringAsync("http://ipv4.icanhazip.com");
                var ipA = IPAddress.Parse(unparsedIpA.Trim());

                // get a new identity
                IPAddress ipB;
                int attempts = 0;
                do
                {
                    attempts++;
                    await proxy.GetNewIdentityAsync();
                    var unparsedIpB = await httpClient.GetStringAsync("http://ipv4.icanhazip.com");
                    ipB = IPAddress.Parse(unparsedIpB.Trim());
                }
                while (Equals(ipA, ipB) && attempts < 5);

                // Assert
                Assert.Equal(AddressFamily.InterNetwork, ipA.AddressFamily);
                Assert.Equal(AddressFamily.InterNetwork, ipB.AddressFamily);
            }
        }
    }
}

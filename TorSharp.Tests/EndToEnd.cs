using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Threading.Tasks;
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
                te.DeleteOnDispose = false;
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
                te.DeleteOnDispose = false;
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

            var httpClient = new HttpClient(handler);
            var proxy = new TorSharpProxy(settings);

            // Act
            // download tools
            await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();

            // start the proxy
            await proxy.ConfigureAndStartAsync();

            // execute some HTTP requests
            var unparsedIpA = await httpClient.GetStringAsync("http://ipv4.icanhazip.com");
            await proxy.GetNewIdentityAsync();
            var unparsedIpB = await httpClient.GetStringAsync("http://ipv4.icanhazip.com");

            // stop
            proxy.Stop();

            // Assert
            var ipA = IPAddress.Parse(unparsedIpA.Trim());
            var ipB = IPAddress.Parse(unparsedIpB.Trim());
            Assert.Equal(AddressFamily.InterNetwork, ipA.AddressFamily);
            Assert.Equal(AddressFamily.InterNetwork, ipB.AddressFamily);
            Assert.NotEqual(ipA, ipB);
        }
    }
}

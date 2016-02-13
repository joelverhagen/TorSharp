using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace Knapcode.TorSharp.Tests
{
    public class EndToEnd
    {
        [Fact]
        public async Task EndToEnd_VirtualDesktopRunner()
        {
            // Arrange
            var settings = new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorZipped"),
                ExtractedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorExtracted"),
                PrivoxyPort = 1337,
                TorSocksPort = 1338,
                TorControlPort = 1339,
                TorControlPassword = "foobar",
                ToolRunnerType = ToolRunnerType.VirtualDesktop,
                ReloadTools = true
            };

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

            // Assert
            var ipA = IPAddress.Parse(unparsedIpA.Trim());
            var ipB = IPAddress.Parse(unparsedIpB.Trim());
            Assert.Equal(AddressFamily.InterNetwork, ipA.AddressFamily);
            Assert.Equal(AddressFamily.InterNetwork, ipB.AddressFamily);
            Assert.NotEqual(ipA, ipB);
        }
    }
}

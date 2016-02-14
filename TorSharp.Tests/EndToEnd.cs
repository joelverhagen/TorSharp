using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tests.TestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests
{
    public class EndToEnd
    {
        private readonly ITestOutputHelper _output;

        public EndToEnd(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task EndToEnd_VirtualDesktopToolRunner()
        {
            using (var te = TestEnvironment.Initialize(_output))
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
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                var settings = te.BuildSettings();
                settings.ToolRunnerType = ToolRunnerType.Simple;

                // Act & Assert
                await ExecuteEndToEndTestAsync(settings);
            }
        }

        private async Task ExecuteEndToEndTestAsync(TorSharpSettings settings)
        {
            // Arrange
            using (var proxy = new TorSharpProxy(settings))
            {
                _output.WriteLine($"Initialized proxy with tool runner type {settings.ToolRunnerType}");

                // Act
                await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();
                _output.WriteLine("The tools have been fetched");
                await proxy.ConfigureAndStartAsync();
                _output.WriteLine("The proxy has been started");

                // get the first identity
                var ipA = await GetCurrentIpAddressAsync(settings);
                await proxy.GetNewIdentityAsync();
                _output.WriteLine("Get new identity succeeded");
                var ipB = await GetCurrentIpAddressAsync(settings);
                
                // Assert
                Assert.Equal(AddressFamily.InterNetwork, ipA.AddressFamily);
                Assert.Equal(AddressFamily.InterNetwork, ipB.AddressFamily);
            }
        }

        private async Task<IPAddress> GetCurrentIpAddressAsync(TorSharpSettings settings)
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxyPort))
            };

            using (var httpClient = new HttpClient(handler))
            {
                var ip = (await httpClient.GetStringAsync("http://ipv4.icanhazip.com")).Trim();
                _output.WriteLine($"Get IP succeeded: {ip}");
                return IPAddress.Parse(ip);
            }
        }
    }
}

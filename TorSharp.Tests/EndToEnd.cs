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

        private async Task ExecuteEndToEndTestAsync(TorSharpSettings settings)
        {
            // Arrange
            using (var proxy = new TorSharpProxy(settings))
            {
                // Act
                await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();
                await proxy.ConfigureAndStartAsync();

                // get the first identity
                var ipA = await GetCurrentIpAddressAsync(proxy, settings);

                // get the second identity
                int attempts = 0;
                IPAddress ipB = ipA;
                while (attempts < 10 && ipA.Equals(ipB))
                {
                    attempts++;
                    await proxy.GetNewIdentityAsync();
                    ipB = await GetCurrentIpAddressAsync(proxy, settings);
                }

                _output.WriteLine($"Got IP B: {ipB}");

                // Assert
                Assert.Equal(AddressFamily.InterNetwork, ipA.AddressFamily);
                Assert.Equal(AddressFamily.InterNetwork, ipB.AddressFamily);
                Assert.NotEqual(ipA, ipB);
            }
        }

        private async Task<IPAddress> GetCurrentIpAddressAsync(TorSharpProxy proxy, TorSharpSettings settings)
        {
            int attempts = 0;
            while (true)
            {
                attempts++;

                if (attempts > 0)
                {
                    await proxy.GetNewIdentityAsync();
                }

                try
                {
                    return await GetCurrentIpAddressAsync(settings);
                }
                catch(Exception e)
                {
                    if (attempts >= 3)
                    {
                        throw;
                    }

                    _output.WriteLine($"Get IP attempt {attempts} failed:{Environment.NewLine}{e}");
                }
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
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                var ip = (await httpClient.GetStringAsync("http://ipv4.icanhazip.com")).Trim();
                _output.WriteLine($"Get IP succeeded: {ip}");
                return IPAddress.Parse(ip);
            }
        }
    }
}

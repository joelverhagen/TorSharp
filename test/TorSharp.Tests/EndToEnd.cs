using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tests.TestSupport;
using Proxy;
using Proxy.Configurations;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests
{
    [Collection(HttpCollection.Name)]
    public class EndToEnd
    {
        private readonly HttpFixture _httpFixture;
        private readonly ITestOutputHelper _output;

        public EndToEnd(HttpFixture httpFixture, ITestOutputHelper output)
        {
            _httpFixture = httpFixture;
            _output = output;
        }

        [PlatformFact(osPlatform: nameof(TorSharpOSPlatform.Windows))]
        [DisplayTestMethodName]
        public async Task VirtualDesktopToolRunner_ConfigurationPathsWithSpaces()
        {
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                te.BaseDirectory = Path.Combine(te.BaseDirectory, "Path With Spaces");
                var settings = te.BuildSettings();
                settings.ToolRunnerType = ToolRunnerType.VirtualDesktop;

                // Act & Assert
                await ExecuteEndToEndTestAsync(settings);
            }
        }

        [Fact]
        [DisplayTestMethodName]
        public async Task SimpleToolRunner_ConfigurationPathsWithSpaces()
        {
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                te.BaseDirectory = Path.Combine(te.BaseDirectory, "Path With Spaces");
                var settings = te.BuildSettings();
                settings.ToolRunnerType = ToolRunnerType.Simple;

                // Act & Assert
                await ExecuteEndToEndTestAsync(settings);
            }
        }

        [PlatformFact(osPlatform: nameof(TorSharpOSPlatform.Windows))]
        [DisplayTestMethodName]
        public async Task VirtualDesktopToolRunner_EndToEnd()
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
        [DisplayTestMethodName]
        public async Task SimpleToolRunner_EndToEnd()
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

        [Fact]
        [DisplayTestMethodName]
        public async Task TorHttpsProxy_EndToEnd()
        {
            var proxyUsername = "proxy-username";
            var proxyPassword = "proxy-password";
            using (var proxyPort = ReservedPort.Reserve())
            using (var ptp = new PassThroughProxy(proxyPort.Port, new Configuration(
                new Server(proxyPort.Port, rejectHttpProxy: true),
                new Authentication(true, proxyUsername, proxyPassword),
                new Firewall(false, new Rule[0]))))
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                var settings = te.BuildSettings();
                settings.TorSettings.HttpsProxyHost = "localhost";
                settings.TorSettings.HttpsProxyPort = proxyPort.Port;
                settings.TorSettings.HttpsProxyUsername = proxyUsername;
                settings.TorSettings.HttpsProxyPassword = proxyPassword;
                settings.ToolRunnerType = ToolRunnerType.Simple;

                // Act & Assert
                await ExecuteEndToEndTestAsync(settings);
            }
        }

        private async Task ExecuteEndToEndTestAsync(TorSharpSettings settings)
        {
            // Arrange
            using (var httpClient = new HttpClient())
            using (var proxy = new TorSharpProxy(settings))
            {
                _output.WriteLine(settings);

                // Act
                var fetcher = _httpFixture.GetTorSharpToolFetcher(settings, httpClient);
                await fetcher.FetchAsync();
                _output.WriteLine("The tools have been fetched");
                await proxy.ConfigureAndStartAsync();
                _output.WriteLine("The proxy has been started");

                // get the first identity
                var ipA = await GetCurrentIpAddressAsync(proxy, settings);
                await proxy.GetNewIdentityAsync();
                _output.WriteLine("Get new identity succeeded");
                var ipB = await GetCurrentIpAddressAsync(proxy, settings);
                
                // Assert
                Assert.Equal(AddressFamily.InterNetwork, ipA.AddressFamily);
                Assert.Equal(AddressFamily.InterNetwork, ipB.AddressFamily);

                var zippedDir = new DirectoryInfo(settings.ZippedToolsDirectory);
                Assert.True(zippedDir.Exists, "The zipped tools directory should exist.");
                Assert.Empty(zippedDir.EnumerateDirectories());
                Assert.Equal(2, zippedDir.EnumerateFiles().Count());

                var extractedDir = new DirectoryInfo(settings.ExtractedToolsDirectory);
                Assert.True(extractedDir.Exists, "The extracted tools directory should exist.");
                Assert.Equal(2, extractedDir.EnumerateDirectories().Count());
                Assert.Empty(extractedDir.EnumerateFiles());
            }
        }

        private async Task<IPAddress> GetCurrentIpAddressAsync(TorSharpProxy proxy, TorSharpSettings settings)
        {
            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {

                    var handler = new HttpClientHandler
                    {
                        Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxySettings.Port))
                    };

                    using (handler)
                    using (var httpClient = new HttpClient(handler))
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(60);

                        var ip = (await httpClient.GetStringAsync("https://api.ipify.org")).Trim();
                        _output.WriteLine($"Get IP succeeded: {ip}");
                        return IPAddress.Parse(ip);
                    }
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    _output.WriteLine($"[Attempt {attempt}] An exception was thrown while fetching an IP. Retrying." + Environment.NewLine + ex);
                    await proxy.GetNewIdentityAsync();
                }
            }

            throw new NotImplementedException();
        }
    }
}

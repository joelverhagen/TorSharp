using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tests.TestSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Proxy;
using Proxy.Configurations;
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
        public async Task TorSharpToolFetcher_CheckForUpdates()
        {
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                var settings = te.BuildSettings();

                using (var httpClient = new HttpClient())
                using (var proxy = new TorSharpProxy(settings))
                {
                    TraceSettings(settings);
                    var fetcher = new TorSharpToolFetcher(settings, httpClient);
                    var fakeOldPrivoxy = Path.Combine(settings.ZippedToolsDirectory, "privoxy-0.0.1.zip");

                    // Act
                    var initial = await fetcher.CheckForUpdatesAsync();
                    await fetcher.FetchAsync(initial);
                    File.Move(initial.Privoxy.DestinationPath, fakeOldPrivoxy);
                    var newerVersion = await fetcher.CheckForUpdatesAsync();
                    await fetcher.FetchAsync(newerVersion);
                    var upToDate = await fetcher.CheckForUpdatesAsync();

                    // Assert
                    Assert.True(initial.HasUpdate);
                    Assert.Equal(ToolUpdateStatus.NoLocalVersion, initial.Privoxy.Status);
                    Assert.Equal(ToolUpdateStatus.NoLocalVersion, initial.Tor.Status);

                    Assert.True(newerVersion.HasUpdate);
                    Assert.Equal(ToolUpdateStatus.NewerVersionAvailable, newerVersion.Privoxy.Status);
                    Assert.Equal(ToolUpdateStatus.NoUpdateAvailable, newerVersion.Tor.Status);

                    Assert.False(upToDate.HasUpdate);
                    Assert.Equal(ToolUpdateStatus.NoUpdateAvailable, upToDate.Privoxy.Status);
                    Assert.Equal(ToolUpdateStatus.NoUpdateAvailable, upToDate.Tor.Status);
                }
            }
        }

        [Fact]
        public async Task TorSharpToolFetcher_UseExistingTools()
        {
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                var settings = te.BuildSettings();
                settings.ReloadTools = true;
                settings.UseExistingTools = true;

                using (var httpClientHandler = new HttpClientHandler())
                using (var requestCountHandler = new RequestCountHandler { InnerHandler = httpClientHandler })
                using (var httpClient = new HttpClient(requestCountHandler))
                using (var proxy = new TorSharpProxy(settings))
                {
                    TraceSettings(settings);
                    await new TorSharpToolFetcher(settings, httpClient).FetchAsync();
                    var requestCount = requestCountHandler.RequestCount;

                    // Act
                    await new TorSharpToolFetcher(settings, httpClient).FetchAsync();

                    // Assert
                    Assert.True(requestCount > 0, "The should be at least one request.");
                    Assert.Equal(requestCount, requestCountHandler.RequestCount);
                    Assert.NotNull(ToolUtility.GetLatestToolOrNull(settings, ToolUtility.PrivoxySettings));
                    Assert.NotNull(ToolUtility.GetLatestToolOrNull(settings, ToolUtility.TorSettings));
                }
            }
        }

        [Fact]
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

        [Fact]
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
                TraceSettings(settings);

                // Act
                await new TorSharpToolFetcher(settings, httpClient).FetchAsync();
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

                var zippedDir = new DirectoryInfo(settings.ZippedToolsDirectory);
                Assert.True(zippedDir.Exists, "The zipped tools directory should exist.");
                Assert.Equal(0, zippedDir.EnumerateDirectories().Count());
                Assert.Equal(2, zippedDir.EnumerateFiles().Count());

                var extractedDir = new DirectoryInfo(settings.ExtractedToolsDirectory);
                Assert.True(extractedDir.Exists, "The extracted tools directory should exist.");
                Assert.Equal(2, extractedDir.EnumerateDirectories().Count());
                Assert.Equal(0, extractedDir.EnumerateFiles().Count());
            }
        }

        private void TraceSettings(TorSharpSettings settings)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                Converters =
                {
                    new StringEnumConverter()
                }
            };

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented, serializerSettings);

            _output.WriteLine("TorSharpSettings:" + Environment.NewLine + json);
        }

        private async Task<IPAddress> GetCurrentIpAddressAsync(TorSharpSettings settings)
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
    }
}

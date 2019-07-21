using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tests.TestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests
{
    public class TorSharpFetcherTests
    {
        private readonly ITestOutputHelper _output;

        public TorSharpFetcherTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TorSharpToolFetch_AllResultsAreWorking()
        {
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                var settings = te.BuildSettings();
                settings.ToolDownloadStrategy = ToolDownloadStrategy.All;

                using (var httpClientHandler = new HttpClientHandler())
                using (var loggingHandler = new LoggingHandler(_output) { InnerHandler = httpClientHandler })
                using (var httpClient = new HttpClient(loggingHandler))
                using (var proxy = new TorSharpProxy(settings))
                {
                    _output.WriteLine(settings);
                    var fetcher = new TorSharpToolFetcher(settings, httpClient);

                    // Act
                    var updates = await fetcher.CheckForUpdatesAsync();

                    // Assert
                    Assert.NotNull(updates);
                    _output.WriteLine("Privoxy URL: " + updates.Privoxy.LatestDownload.Url.AbsoluteUri);
                    _output.WriteLine("Tor URL: " + updates.Tor.LatestDownload.Url.AbsoluteUri);
                }
            }
        }

        [Theory]
        [InlineData(ToolDownloadStrategy.First)]
        [InlineData(ToolDownloadStrategy.Latest)]
        public async Task TorSharpToolFetcher_CheckForUpdates()
        {
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                var settings = te.BuildSettings();

                using (var httpClient = new HttpClient())
                using (var proxy = new TorSharpProxy(settings))
                {
                    _output.WriteLine(settings);
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
                    _output.WriteLine(settings);
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
    }
}

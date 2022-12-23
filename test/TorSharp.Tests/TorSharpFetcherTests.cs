using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tests.TestSupport;
using xRetry;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests
{
    [Collection(HttpCollection.Name)]
    public class TorSharpFetcherTests
    {
        private readonly HttpFixture _httpFixture;
        private readonly ITestOutputHelper _output;

        public TorSharpFetcherTests(HttpFixture httpFixture, ITestOutputHelper output)
        {
            _httpFixture = httpFixture;
            _output = output;
        }

        [RetryTheory]
        [MemberData(nameof(Platforms))]
        [DisplayTestMethodName]
        public async Task TorSharpToolFetch_AllResultsAreWorking(TorSharpOSPlatform osPlatform, TorSharpArchitecture architecture)
        {
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                var settings = te.BuildSettings();
                settings.OSPlatform = osPlatform;
                settings.Architecture = architecture;
                settings.ToolDownloadStrategy = ToolDownloadStrategy.All;

                using (var httpClientHandler = new HttpClientHandler())
                using (var loggingHandler = new LoggingHandler(_output) { InnerHandler = httpClientHandler })
                using (var httpClient = new HttpClient(loggingHandler))
                using (var proxy = new TorSharpProxy(settings))
                {
                    _output.WriteLine(settings);
                    var fetcher = _httpFixture.GetTorSharpToolFetcher(settings, httpClient);

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
        [DisplayTestMethodName]
        public async Task TorSharpToolFetcher_CheckForUpdates(ToolDownloadStrategy strategy)
        {
            using (var te = TestEnvironment.Initialize(_output))
            {
                // Arrange
                var settings = te.BuildSettings();
                settings.ToolDownloadStrategy = strategy;

                using (var httpClientHandler = new HttpClientHandler())
                using (var loggingHandler = new LoggingHandler(_output) { InnerHandler = httpClientHandler })
                using (var httpClient = new HttpClient(loggingHandler))
                using (var proxy = new TorSharpProxy(settings))
                {
                    _output.WriteLine(settings);
                    var fetcher = _httpFixture.GetTorSharpToolFetcher(settings, httpClient);
                    var initial = await fetcher.CheckForUpdatesAsync();
                    await fetcher.FetchAsync(initial);

                    var prefix = ToolUtility.GetPrivoxyToolSettings(settings).Prefix;
                    var extension = Path.GetExtension(initial.Privoxy.DestinationPath);
                    var fakeOldPrivoxy = Path.Combine(settings.ZippedToolsDirectory, $"{prefix}0.0.1{extension}");
                    File.Move(initial.Privoxy.DestinationPath, fakeOldPrivoxy);

                    // Act
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
        [DisplayTestMethodName]
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
                using (var loggingHandler = new LoggingHandler(_output) { InnerHandler = requestCountHandler })
                using (var httpClient = new HttpClient(requestCountHandler))
                using (var proxy = new TorSharpProxy(settings))
                {
                    _output.WriteLine(settings);
                    var fetcherA = _httpFixture.GetTorSharpToolFetcher(settings, httpClient);
                    await fetcherA.FetchAsync();
                    var requestCount = requestCountHandler.RequestCount;

                    // Act
                    var fetcherB = _httpFixture.GetTorSharpToolFetcher(settings, httpClient);
                    await fetcherB.FetchAsync();

                    // Assert
                    Assert.True(requestCount > 0, "The should be at least one request.");
                    Assert.Equal(requestCount, requestCountHandler.RequestCount);
                    Assert.NotNull(ToolUtility.GetLatestToolOrNull(settings, ToolUtility.GetPrivoxyToolSettings(settings)));
                    Assert.NotNull(ToolUtility.GetLatestToolOrNull(settings, ToolUtility.GetTorToolSettings(settings)));
                }
            }
        }

        public static IEnumerable<object[]> Platforms
        {
            get
            {
                foreach (var osPlatform in Enum.GetValues(typeof(TorSharpOSPlatform)).Cast<TorSharpOSPlatform>())
                {
                    if (osPlatform == TorSharpOSPlatform.Unknown)
                    {
                        continue;
                    }

                    foreach (var architecture in Enum.GetValues(typeof(TorSharpArchitecture)).Cast<TorSharpArchitecture>())
                    {
                        if (architecture == TorSharpArchitecture.Unknown)
                        {
                            continue;
                        }

                        yield return new object[] { osPlatform, architecture };
                    }
                }
            }
        }
    }
}

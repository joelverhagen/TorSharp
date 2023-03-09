using System.Net.Http;
using System.Threading.Tasks;

using Knapcode.TorSharp.Tools.Tor;

using Xunit;

namespace Knapcode.TorSharp.Tests.ARM64
{
    public class FetcherTests
    {
        private readonly HttpClient HttpClient = new HttpClient();

        [Fact]
        public async Task TorSharpToolFetcher_TestArm32()
        {
            var settings = new TorSharpSettings()
            {
                OSPlatform = TorSharpOSPlatform.Linux,
                Architecture = TorSharpArchitecture.Arm32,
                AllowUnofficialSources = true
            };

            var fetcher = new TorFetcher(settings, HttpClient);
            var result = await fetcher.GetLatestAsync();

            Assert.NotNull(result);
            Assert.NotNull(result.Url);
        }

        [Fact]
        public async Task TorSharpToolFetcher_TestArm64()
        {
            var settings = new TorSharpSettings()
            {
                OSPlatform = TorSharpOSPlatform.Linux,
                Architecture = TorSharpArchitecture.Arm64,
                AllowUnofficialSources = true
            };

            var fetcher = new TorFetcher(settings, HttpClient);
            var result =  await fetcher.GetLatestAsync();
            
            Assert.NotNull(result);
            Assert.NotNull(result.Url);
        }
    }
}

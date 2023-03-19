using System.Net.Http;
using System.Threading.Tasks;

using Knapcode.TorSharp.Tools.Tor;

using Xunit;

namespace Knapcode.TorSharp.Tests.ARM
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

        //[Fact]
        //public async Task TorSharpToolFetcher_TestDownload()
        //{
        //    var settings = new TorSharpSettings()
        //    {
        //        OSPlatform = TorSharpOSPlatform.Linux,
        //        Architecture = TorSharpArchitecture.Arm64,
        //        AllowUnofficialSources = true
        //    };
        //    settings.PrivoxySettings.Disable = true;

        //    var toolFetcher = new TorSharpToolFetcher(settings, HttpClient);
        //    await toolFetcher.FetchAsync();

        //    var ts = new TorSharpProxy(settings);
        //    await ts.ConfigureAsync();
        //}
    }
}

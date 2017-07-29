using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Sandbox
{
    internal class Program
    {
        private static void Main()
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            // configure
            var settings = new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorZipped"),
                ExtractedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorExtracted"),
                PrivoxyPort = 1337,
                TorSocksPort = 1338,
                TorControlPort = 1339,
                TorControlPassword = "foobar",
#if NET45
                ToolRunnerType = ToolRunnerType.VirtualDesktop,
#else
                ToolRunnerType = ToolRunnerType.Simple,
#endif
                WaitForTestEndpoint = true
            };

            // download tools
            await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();

            // execute
            using (var proxy = new TorSharpProxy(settings))
            using (var handler = proxy.CreateHttpClientHandler())
            using (var httpClient = new HttpClient(handler))
            {
                await proxy.ConfigureAndStartAsync();
                Console.WriteLine(await httpClient.GetStringAsync("http://icanhazip.com"));

                await proxy.GetNewIdentityAsync();
                Console.WriteLine(await httpClient.GetStringAsync("http://icanhazip.com"));
            }
        }
    }
}
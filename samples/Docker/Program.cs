using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Knapcode.TorSharp.Sandbox
{
    internal class Program
    {
        private static async Task Main()
        {
            var settings = new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorZipped"),
                ExtractedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorExtracted"),
                PrivoxySettings =
                {
                    Port = 18118,
                    // ExecutablePathOverride = "privoxy",
                },
                TorSettings =
                {
                    SocksPort = 19050,
                    ControlPort = 19051,
                    ControlPassword = "foobar",
                },
            };

            using (var httpClient = new HttpClient())
            {
                var fetcher = new TorSharpToolFetcher(settings, httpClient);
                Console.WriteLine("Checking for updates...");
                var updates = await fetcher.CheckForUpdatesAsync();
                if (updates.HasUpdate)
                {
                    Console.WriteLine("Fetching updates...");
                    await fetcher.FetchAsync(updates);
                }
            }

            Console.WriteLine("Starting TorSharp...");
            using (var proxy = new TorSharpProxy(settings))
            {
                var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxySettings.Port))
                };

                using (handler)
                using (var httpClient = new HttpClient(handler))
                {
                    await proxy.ConfigureAndStartAsync();
                    Console.WriteLine("Output: " + await httpClient.GetStringAsync("http://api.ipify.org"));
                }

                proxy.Stop();
            }
        }
    }
}

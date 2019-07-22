using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Sandbox
{
    internal class Program
    {
        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            // configure
            var settings = new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorZipped"),
                ExtractedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorExtracted"),
                PrivoxySettings =
                {
                    Port = 1337,
                },
                TorSettings =
                {
                    SocksPort = 1338,
                    ControlPort = 1339,
                    ControlPassword = "foobar",
                },
                ToolRunnerType = ToolRunnerType.VirtualDesktop,
            };

            // download tools
            using (var httpClient = new HttpClient())
            {
                var fetcher = new TorSharpToolFetcher(settings, httpClient);
                var updates = await fetcher.CheckForUpdatesAsync();
                Console.WriteLine($"Current Privoxy: {updates.Privoxy.LocalVersion?.ToString() ?? "(none)"}");
                Console.WriteLine($" Latest Privoxy: {updates.Privoxy.LatestDownload.Version}");
                Console.WriteLine();
                Console.WriteLine($"Current Tor: {updates.Tor.LocalVersion?.ToString() ?? "(none)"}");
                Console.WriteLine($" Latest Tor: {updates.Tor.LatestDownload.Version}");
                Console.WriteLine();
                if (updates.HasUpdate)
                {
                    await fetcher.FetchAsync(updates);
                }
            }

            // execute
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
                    Console.WriteLine(await httpClient.GetStringAsync("http://api.ipify.org"));
                    await proxy.GetNewIdentityAsync();
                    Console.WriteLine(await httpClient.GetStringAsync("http://api.ipify.org"));
                }

                proxy.Stop();
            }
        }
    }
}
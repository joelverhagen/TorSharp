using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#if NETCOREAPP
using System.Runtime.InteropServices;
#endif

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
                    Port = 18118,
                },
                TorSettings =
                {
                    SocksPort = 19050,
                    ControlPort = 19051,
                    ControlPassword = "foobar",
                },
            };

            // output runtime information
            var message = new StringBuilder();
            message.Append($"Running the sample on {settings.OSPlatform} OS and {settings.Architecture} architecture.");
#if NETCOREAPP
            message.Append($" OS description: {RuntimeInformation.OSDescription}.");
#endif
            Console.WriteLine(message.ToString());
            Console.WriteLine();

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

                    using (var controlClient = await proxy.GetControlClientAsync())
                    {
                        var read = await controlClient.GetTrafficReadAsync();
                        Console.WriteLine("Bytes read    : {0}", read);
                        var written = await controlClient.GetTrafficWrittenAsync();
                        Console.WriteLine("Bytes written : {0}", written);
                    }
                }

                proxy.Stop();
            }
        }
    }
}
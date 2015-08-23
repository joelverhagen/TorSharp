using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tools;
using Knapcode.TorSharp.Tools.Privoxy;
using Knapcode.TorSharp.Tools.Tor;

namespace Knapcode.TorSharp.Sandbox
{
    internal class Program
    {
        private static void Main()
        {
            // configure
            var settings = new TorSharpSettings
            {
                ReloadTools = false,
                ZippedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorSharp", "ZippedTools"),
                ExtractedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorSharp", "ExtractedTools"),
                PrivoxyPort = 1337,
                TorSocksPort = 1338,
                TorControlPort = 1339,
                TorControlPassword = "foobar"
            };

            var proxy = new TorSharpProxy(settings);
            var handler = new HttpClientHandler { Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxyPort)) };
            var httpClient = new HttpClient(handler);

            // download tools
            if (settings.ReloadTools)
            {
                Directory.Delete(settings.ZippedToolsDirectory, true);
            }
            DownloadFileAsync(settings, new TorFetcher(new HttpClient())).Wait();
            DownloadFileAsync(settings, new PrivoxyFetcher(new HttpClient())).Wait();

            // execute
            proxy.ConfigureAndStartAsync().Wait();
            Console.WriteLine(httpClient.GetStringAsync("http://v4.ipv6-test.com/api/myip.php").Result);
            proxy.GetNewIdentityAsync().Wait();
            Console.WriteLine(httpClient.GetStringAsync("http://v4.ipv6-test.com/api/myip.php").Result);
            proxy.Stop();
        }

        private static async Task DownloadFileAsync(TorSharpSettings settings, IFileFetcher fetcher)
        {
            Directory.CreateDirectory(settings.ZippedToolsDirectory);
            var file = await fetcher.GetLatestAsync();
            string filePath = Path.Combine(settings.ZippedToolsDirectory, file.Name);
            if (!File.Exists(filePath))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var contentStream = await file.GetContentAsync();
                    await contentStream.CopyToAsync(fileStream);
                }
            }
        }
    }
}
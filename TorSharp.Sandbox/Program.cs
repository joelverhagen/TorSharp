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

            // download tools
            new ToolFetcher(settings, new HttpClient()).FetchAsync().Wait();

            // execute
            var proxy = new TorSharpProxy(settings);
            var handler = new HttpClientHandler { Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxyPort)) };
            var httpClient = new HttpClient(handler);
            proxy.ConfigureAndStartAsync().Wait();
            Console.WriteLine(httpClient.GetStringAsync("http://icanhazip.com/").Result.Trim());
            proxy.GetNewIdentityAsync().Wait();
            Console.WriteLine(httpClient.GetStringAsync("http://icanhazip.com/").Result.Trim());
            proxy.Stop();
        }
    }
}
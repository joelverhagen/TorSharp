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
                ReloadTools = true,
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
                ToolRunnerType = ToolRunnerType.VirtualDesktop
            };

            // download tools
            await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();

            // execute
            var proxy = new TorSharpProxy(settings);
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxySettings.Port))
            };
            var httpClient = new HttpClient(handler);
            await proxy.ConfigureAndStartAsync();
            Console.WriteLine(await httpClient.GetStringAsync("http://api.ipify.org"));
            await proxy.GetNewIdentityAsync();
            Console.WriteLine(await httpClient.GetStringAsync("http://api.ipify.org"));
            proxy.Stop();
        }
    }
}
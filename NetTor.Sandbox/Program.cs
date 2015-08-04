using System;
using System.IO;
using System.Net;
using System.Net.Http;

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
            DownloadFile(settings, "https://www.torproject.org/dist/torbrowser/4.5.3/tor-win32-0.2.6.9.zip", "tor-win32-0.2.6.9.zip");
            DownloadFile(settings, "http://sourceforge.net/projects/ijbswa/files/Win32/3.0.23%20%28stable%29/privoxy-3.0.23.zip/download", "privoxy-3.0.23.zip");

            // execute
            proxy.ConfigureAndStartAsync().Wait();
            Console.WriteLine(httpClient.GetStringAsync("http://v4.ipv6-test.com/api/myip.php").Result);
            proxy.GetNewIdentityAsync().Wait();
            Console.WriteLine(httpClient.GetStringAsync("http://v4.ipv6-test.com/api/myip.php").Result);
            proxy.Stop();
        }

        private static void DownloadFile(TorSharpSettings settings, string url, string name)
        {
            Directory.CreateDirectory(settings.ZippedToolsDirectory);
            string destination = Path.Combine(settings.ZippedToolsDirectory, name);
            if (!File.Exists(destination))
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(url, destination);
                }
            }
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Knapcode.NetTor.Sandbox
{
    internal class Program
    {
        private static void Main()
        {       
            var netTorProxy = new NetTorProxy(new NetTorSettings
            {
                ReloadTools = false,
                ZippedToolsDirectory = @"ZippedTools",
                ExtractedToolsDirectory = Path.Combine(Path.GetTempPath(), "NetTor"),
                PrivoxyPort = 1337,
                TorSocksPort = 1338,
                TorControlPort = 1339,
                TorControlPassword = "foobar"
            });

            netTorProxy.ConfigureAndStartAsync().Wait();

            {
                var handler = new HttpClientHandler { Proxy = new WebProxy(new Uri("http://localhost:1337")) };
                var client = new HttpClient(handler);
                Console.WriteLine(client.GetStringAsync("http://v4.ipv6-test.com/api/myip.php").Result);
            }

            netTorProxy.GetNewIdentityAsync().Wait();

            {
                var handler = new HttpClientHandler { Proxy = new WebProxy(new Uri("http://localhost:1337")) };
                var client = new HttpClient(handler);
                Console.WriteLine(client.GetStringAsync("http://v4.ipv6-test.com/api/myip.php").Result);
            }

            netTorProxy.Stop();
        }
    }
}
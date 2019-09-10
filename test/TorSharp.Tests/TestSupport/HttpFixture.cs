using System;
using System.IO;
using System.Net.Http;
using Knapcode.TorSharp.Tools;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class HttpFixture : IDisposable
    {
        private readonly string _cacheDirectory;

        public HttpFixture()
        {
            _cacheDirectory = Path.Combine(
                Path.GetTempPath(),
                "Knapcode.TorSharp.Tests",
                "cache",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_cacheDirectory);
        }

        internal ISimpleHttpClient GetSimpleHttpClient(HttpClient httpClient)
        {
            return new CachingSimpleHttpClient(httpClient, _cacheDirectory);
        }

        public TorSharpToolFetcher GetTorSharpToolFetcher(TorSharpSettings settings, HttpClient httpClient)
        {
            return new TorSharpToolFetcher(
                settings,
                httpClient,
                GetSimpleHttpClient(httpClient),
                new ConsoleProgress());
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_cacheDirectory, recursive: true);
            }
            catch
            {
                // Not much we can do here.
            }
        }

        private class ConsoleProgress : IProgress<DownloadProgress>
        {
            public void Report(DownloadProgress p)
            {
                Console.WriteLine($"{p.RequestUri.AbsoluteUri}: {p.Complete} / {(p.Total.HasValue ? p.Total.Value.ToString() : "???")}");
            }
        }
    }
}

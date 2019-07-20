using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools.Tor
{
    public class TorFetcher : IFileFetcher
    {
        private static readonly Uri BaseUrl = new Uri("https://dist.torproject.org/torbrowser/");
        private readonly HttpClient _httpClient;

        public TorFetcher(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DownloadableFile> GetLatestAsync()
        {
            var downloadableFile = await FetcherHelpers.GetLatestDownloadableFileAsync(
                _httpClient,
                BaseUrl,
                @"tor-win32-(?<Version>[\d\.]+)\.zip$",
                CancellationToken.None).ConfigureAwait(false);

            if (downloadableFile == null)
            {
                throw new TorSharpException($"No version of Tor could be found under base URL {BaseUrl}.");
            }

            return downloadableFile;
        }
    }
}
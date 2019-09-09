using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    internal interface ISimpleHttpClient
    {
        Task DownloadToFileAsync(Uri requestUri, string path);
    }

    internal class SimpleHttpClient : ISimpleHttpClient
    {
        private readonly HttpClient _httpClient;

        public SimpleHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task DownloadToFileAsync(Uri requestUri, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create))
            using (var contentStream = await _httpClient.GetStreamAsync(requestUri).ConfigureAwait(false))
            {
                await contentStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }
        }
    }
}

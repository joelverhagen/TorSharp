using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    internal interface ISimpleHttpClient
    {
        Task DownloadToFileAsync(Uri requestUri, string path, IProgress<DownloadProgress> progress);
    }

    internal class SimpleHttpClient : ISimpleHttpClient
    {
        private readonly HttpClient _httpClient;

        public SimpleHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task DownloadToFileAsync(Uri requestUri, string path, IProgress<DownloadProgress> progress)
        {
            using (var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                using (var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    var totalRead = 0;
                    Report(progress, requestUri, totalRead, response);
                    var buffer = new byte[81920];
                    int read;
                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
                    {
                        totalRead += read;
                        await fileStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                        Report(progress, requestUri, totalRead, response);
                    }
                }
            }
        }

        private static void Report(IProgress<DownloadProgress> progress, Uri requestUri, long totalRead, HttpResponseMessage response)
        {
            progress?.Report(new DownloadProgress(requestUri, totalRead, response.Content?.Headers.ContentLength));
        }
    }
}

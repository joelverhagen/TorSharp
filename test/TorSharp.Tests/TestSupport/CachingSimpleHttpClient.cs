using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tools;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    internal class CachingSimpleHttpClient : ISimpleHttpClient
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, SemaphoreSlim> _pathToSemaphore
            = new Dictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        private readonly ITestOutputHelper _output;
        private readonly ISimpleHttpClient _httpClient;
        private readonly string _cacheDirectory;

        public CachingSimpleHttpClient(ITestOutputHelper output, HttpClient httpClient, string cacheDirectory)
        {
            _output = output;
            _httpClient = new SimpleHttpClient(httpClient);
            _cacheDirectory = cacheDirectory;
        }

        public async Task DownloadToFileAsync(Uri requestUri, string path, IProgress<DownloadProgress> progress)
        {
            string cachePath;
            using (var sha1 = SHA1.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(requestUri.AbsoluteUri);
                var hashBytes = sha1.ComputeHash(inputBytes);
                var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
                cachePath = Path.GetFullPath(Path.Combine(_cacheDirectory, hash));
            }

            SemaphoreSlim semaphore;
            lock (_lock)
            {
                if (!_pathToSemaphore.TryGetValue(cachePath, out semaphore))
                {
                    semaphore = new SemaphoreSlim(1);
                    _pathToSemaphore.Add(cachePath, semaphore);
                }
            }

            await semaphore.WaitAsync();
            try
            {
                if (!File.Exists(cachePath))
                {
                    await _httpClient.DownloadToFileAsync(requestUri, cachePath, progress);
                }

                _output.WriteLine($"Copying {cachePath} to {path}");
                File.Copy(cachePath, path, overwrite: true);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
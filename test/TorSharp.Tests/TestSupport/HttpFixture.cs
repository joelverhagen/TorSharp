using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Knapcode.TorSharp.Tools;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class HttpFixture
    {
        private readonly string _cacheDirectory;

        public HttpFixture()
        {
            var cacheDirectory = Environment.GetEnvironmentVariable("TORSHARP_TEST_CACHE");
            if (string.IsNullOrWhiteSpace(cacheDirectory))
            {
                cacheDirectory = Path.Combine(
                    Path.GetTempPath(),
                    "Knapcode.TorSharp.Tests",
                    "cache");
            }

            _cacheDirectory = Path.GetFullPath(cacheDirectory).TrimEnd(Path.DirectorySeparatorChar);
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

        private class ConsoleProgress : IProgress<DownloadProgress>
        {
            private readonly object _lock = new object();
            private readonly Dictionary<Guid, DownloadProgress> _previousProgress = new Dictionary<Guid, DownloadProgress>();

            public void Report(DownloadProgress current)
            { 
                lock (_lock)
                {
                    int? previousPercent;
                    if (_previousProgress.TryGetValue(current.DownloadId, out var previous))
                    {
                        previousPercent = GetPercent(previous);
                    }
                    else
                    {
                        previousPercent = null;
                    }

                    var currentPercent = GetPercent(current);
                    if (!previousPercent.HasValue || currentPercent != previousPercent)
                    {
                        var sb = new StringBuilder();

                        sb.AppendFormat(
                            "{0}: {1} / {2}",
                            current.RequestUri.AbsoluteUri,
                            current.TotalRead,
                            current.ContentLength.HasValue ? current.ContentLength.Value.ToString() : "???");
                        if (currentPercent.HasValue)
                        {
                            sb.AppendFormat(" ({0}%)", currentPercent);
                        }

                        Console.WriteLine(sb.ToString());
                    }

                    if (current.State != DownloadProgressState.Complete)
                    {
                        _previousProgress[current.DownloadId] = current;
                    }
                    else
                    {
                        _previousProgress.Remove(current.DownloadId);
                    }
                }
            }

            private int? GetPercent(DownloadProgress progress)
            {
                if (!progress.ContentLength.HasValue)
                {
                    return null;
                }

                return (int)(100.0 * progress.TotalRead / progress.ContentLength.Value);
            }
        }
    }
}

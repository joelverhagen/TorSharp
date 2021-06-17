using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Knapcode.TorSharp.Tools.Privoxy
{
    internal class PrivoxyFetcher : IFileFetcher
    {
        private static readonly Uri PrivoxyBaseUrl = new Uri("https://www.privoxy.org/feeds/privoxy-releases.xml");
        private static readonly Uri SourceForgeBaseUrl = new Uri("https://sourceforge.net/projects/ijbswa/rss");
        private static readonly Uri PrivoxyMirrorBaseUrl = new Uri("https://www.silvester.org.uk/privoxy/");

        private readonly HttpClient _httpClient;
        private readonly TorSharpSettings _settings;

        public PrivoxyFetcher(TorSharpSettings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;
        }

        public async Task<DownloadableFile> GetLatestAsync()
        {
            switch (_settings.ToolDownloadStrategy)
            {
                case ToolDownloadStrategy.First:
                    {
                        var results = await GetResultsAsync(takeFirst: true).ConfigureAwait(false);
                        return results[0];
                    }
                case ToolDownloadStrategy.Latest:
                case ToolDownloadStrategy.All:
                    {
                        var results = await GetResultsAsync(takeFirst: false).ConfigureAwait(false);

                        const int maxCount = 3;
                        if (_settings.ToolDownloadStrategy == ToolDownloadStrategy.All
                            && results.Count != maxCount)
                        {
                            throw new TorSharpException($"{maxCount - results.Count} out of the {maxCount} Privoxy URLs is not working.");
                        }

                        return results.OrderByDescending(x => x.Version).First();
                    }
                default:
                    throw new NotImplementedException($"The tool download strategy '{_settings.ToolDownloadStrategy}' is not supported.");
            }
        }

        private async Task<List<DownloadableFile>> GetResultsAsync(bool takeFirst)
        {
            var cts = new CancellationTokenSource();

            var tasks = new List<Task<DownloadableFile>>
            {
                GetLatestOrNullFromPrivoxyRssAsync(cts.Token),
                GetLatestOrNullFromSourceForgeRssAsync(cts.Token),
                GetLatestOrNullFromFileListingAsync(PrivoxyMirrorBaseUrl, cts.Token),
            };
            var results = new List<DownloadableFile>();
            var faults = new List<Task<DownloadableFile>>();

            while (tasks.Any() && (!takeFirst || (takeFirst && results.Count == 0)))
            {
                var nextTask = await Task.WhenAny(tasks).ConfigureAwait(false);
                tasks.Remove(nextTask);

                if (nextTask.Status != TaskStatus.RanToCompletion)
                {
                    faults.Add(nextTask);
                }
                else
                {
                    results.Add(nextTask.Result);
                }
            }

            cts.Cancel();

            if (results.Count == 0)
            {
                if (faults.Any())
                {
                    await Task.WhenAll(faults).ConfigureAwait(false);
                }

                throw new TorSharpException($"No version of Privoxy could be found.");
            }

            return results;
        }

        private async Task<DownloadableFile> GetLatestOrNullFromPrivoxyRssAsync(CancellationToken token)
        {
            return await GetLatestOrNullFromRssAsync(PrivoxyBaseUrl, token).ConfigureAwait(false);
        }

        private async Task<DownloadableFile> GetLatestOrNullFromSourceForgeRssAsync(CancellationToken token)
        {
            var directory = GetRssDirectory(SourceForgeBaseUrl);
            var osBaseUrl = new Uri(SourceForgeBaseUrl.AbsoluteUri + $"?path=/{directory}");

            return await GetLatestOrNullFromRssAsync(osBaseUrl, token).ConfigureAwait(false);
        }

        private async Task<DownloadableFile> GetLatestOrNullFromFileListingAsync(
            Uri baseUrl,
            CancellationToken token)
        {
            var directory = GetFileListingDirectory(baseUrl);
            var osBaseUrl = new Uri(baseUrl, $"{directory}/");
            var fileNamePatternAndFormat = GetFileNamePatternAndFormat(osBaseUrl);

            var downloadableFile = await FetcherHelpers.GetLatestDownloadableFileAsync(
                _httpClient,
                osBaseUrl,
                fileNamePatternAndFormat.Pattern,
                fileNamePatternAndFormat.Format,
                token).ConfigureAwait(false);

            if (downloadableFile == null)
            {
                throw new TorSharpException(
                    $"No version of Privoxy could be found under base URL {osBaseUrl.AbsoluteUri} with pattern " +
                    $"{fileNamePatternAndFormat.Pattern}.");
            }

            return downloadableFile;
        }

        private async Task<DownloadableFile> GetLatestOrNullFromRssAsync(
            Uri baseUrl,
            CancellationToken token)
        {
            var directory = GetRssDirectory(baseUrl);
            var fileNamePatternAndFormat = GetFileNamePatternAndFormat(baseUrl);

            SyndicationFeed syndicationFeed;
            using (var response = await _httpClient.GetAsync(baseUrl, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var streamReader = new StreamReader(stream);
                    var xmlReader = XmlReader.Create(streamReader);
                    syndicationFeed = SyndicationFeed.Load(xmlReader);
                    if (syndicationFeed == null)
                    {
                        return null;
                    }
                }
            }

            var downloadableFile = syndicationFeed
                .Items
                .Where(i => i.Links.Any())
                .Where(i => TitleStartWithDirectory(directory, i))
                .Select(i => GetDownloadableFile(fileNamePatternAndFormat.Pattern, fileNamePatternAndFormat.Format, i))
                .Where(i => i != null)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();

            if (downloadableFile == null)
            {
                throw new TorSharpException(
                    $"No version of Privoxy could be found under base URL {baseUrl.AbsoluteUri} with directory " +
                    $"{directory} and file name pattern {fileNamePatternAndFormat.Pattern}.");
            }

            return downloadableFile;
        }

        private static bool TitleStartWithDirectory(string directory, SyndicationItem item)
        {
            return Regex.IsMatch(
                item.Title.Text,
                $"^/?{directory}/",
                RegexOptions.IgnoreCase);
        }

        private DownloadableFile GetDownloadableFile(
            string fileNamePattern,
            ZippedToolFormat format,
            SyndicationItem item)
        {
            var match = Regex.Match(
                item.Title.Text,
                fileNamePattern,
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            if (!Version.TryParse(match.Groups["Version"].Value, out var parsedVersion))
            {
                return null;
            }

            var downloadUrl = item.Links.First().Uri;
            return new DownloadableFile(parsedVersion, downloadUrl, format);
        }

        private string GetFileListingDirectory(Uri baseUrl)
        {
            string directory = null;
            if (_settings.OSPlatform == TorSharpOSPlatform.Windows)
            {
                directory = "Windows";
            }
            else if (_settings.OSPlatform == TorSharpOSPlatform.Linux)
            {
                directory = "Debian";
            }

            if (directory == null)
            {
                Reject(baseUrl);
            }

            return directory;
        }

        private string GetRssDirectory(Uri baseUrl)
        {
            string directory = null;
            if (_settings.OSPlatform == TorSharpOSPlatform.Windows)
            {
                directory = "Win32";
            }
            else if (_settings.OSPlatform == TorSharpOSPlatform.Linux)
            {
                directory = "Debian";
            }

            if (directory == null)
            {
                Reject(baseUrl);
            }

            return directory;
        }

        private FileNamePatternAndFormat GetFileNamePatternAndFormat(Uri baseUrl)
        {
            var pattern = default(string);
            var format = default(ZippedToolFormat);
            if (_settings.OSPlatform == TorSharpOSPlatform.Windows)
            {
                pattern = @"privoxy[-_](?<Version>[\d\.]+)\.zip$";
                format = ZippedToolFormat.Zip;
            }
            else if (_settings.OSPlatform == TorSharpOSPlatform.Linux)
            {
                if (_settings.Architecture == TorSharpArchitecture.X86)
                {
                    pattern = @"privoxy[-_](?<Version>[\d\.]+)([-_]\d_)?i386\.deb$";
                    format = ZippedToolFormat.Deb;
                }
                else if (_settings.Architecture == TorSharpArchitecture.X64)
                {
                    pattern = @"privoxy[-_](?<Version>[\d\.]+)([-_]\d_)?amd64\.deb$";
                    format = ZippedToolFormat.Deb;
                }
            }

            if (pattern == null)
            {
                Reject(baseUrl);
            }

            return new FileNamePatternAndFormat(pattern, format);
        }

        private void Reject(Uri baseUrl)
        {
            _settings.RejectRuntime($"fetch Privoxy from {baseUrl.AbsoluteUri}");
        }
    }
}
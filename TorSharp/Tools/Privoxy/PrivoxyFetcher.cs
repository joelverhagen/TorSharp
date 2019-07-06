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
    public class PrivoxyFetcher : IFileFetcher
    {
        private static readonly Uri PrivoxyBaseUrl = new Uri("https://www.privoxy.org/feeds/privoxy-releases.xml");
        private static readonly Uri SourceForgeBaseUrl = new Uri("https://sourceforge.net/projects/ijbswa/rss?path=/Win32");
        private static readonly Uri PrivoxyMirrorBaseUrl = new Uri("https://www.silvester.org.uk/privoxy/Windows/");

        private readonly HttpClient _httpClient;

        public PrivoxyFetcher(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DownloadableFile> GetLatestAsync()
        {
            var cts = new CancellationTokenSource();

            var tasks = new List<Task<DownloadableFile>>
            {
                GetLatestOrNullFromRssAsync(PrivoxyBaseUrl, cts.Token, TitleStartWithWin32),
                GetLatestOrNullFromRssAsync(SourceForgeBaseUrl, cts.Token),
                GetLatestOrNullFromFileListingAsync(PrivoxyMirrorBaseUrl, cts.Token),
            };
            var faults = new List<Task<DownloadableFile>>();

            DownloadableFile result = null;
            while (tasks.Any() && result == null)
            {
                var nextTask = await Task.WhenAny(tasks).ConfigureAwait(false);
                tasks.Remove(nextTask);

                if (nextTask.Status != TaskStatus.RanToCompletion)
                {
                    faults.Add(nextTask);
                }
                else
                {
                    result = nextTask.Result;
                }
            }

            cts.Cancel();

            if (result == null)
            {
                if (faults.Any())
                {
                    await Task.WhenAll(faults).ConfigureAwait(false);
                }

                throw new TorSharpException($"No version of Privoxy could be found.");
            }

            return result;
        }

        private async Task<DownloadableFile> GetLatestOrNullFromFileListingAsync(
            Uri baseUrl,
            CancellationToken token)
        {
            return await FetcherHelpers.GetLatestDownloadableFileAsync(
                _httpClient,
                baseUrl,
                @"privoxy-.+\.zip$",
                token);
        }

        private async Task<DownloadableFile> GetLatestOrNullFromRssAsync(
            Uri baseUrl,
            CancellationToken token,
            Func<SyndicationItem, bool> predicate = null)
        {
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

            var latest = syndicationFeed
                .Items
                .Where(i => i.Links.Any())
                .Where(predicate ?? (i => true))
                .OrderByDescending(i => i.PublishDate)
                .FirstOrDefault(TitleEndsWithPrivoxyZip);

            if (latest == null)
            {
                return null;
            }

            var name = latest.Title.Text.Split('/').Last();
            var downloadUrl = latest.Links.First().Uri;
            return new DownloadableFile
            {
                Name = name,
                GetContentAsync = () => _httpClient.GetStreamAsync(downloadUrl),
                Url = downloadUrl,
            };
        }

        private static bool TitleStartWithWin32(SyndicationItem i)
        {
            return i.Title.Text.StartsWith("Win32/", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TitleEndsWithPrivoxyZip(SyndicationItem item)
        {
            return Regex.IsMatch(item.Title.Text, @"privoxy-[\d\.]+\.zip$", RegexOptions.IgnoreCase);
        }
    }
}
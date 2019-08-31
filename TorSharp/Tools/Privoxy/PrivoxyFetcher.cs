using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Knapcode.TorSharp.Tools.Privoxy
{
    public class PrivoxyFetcher : IFileFetcher
    {
        private const string FileNamePattern = @"privoxy[-_](?<Version>[\d\.]+)\.zip$";

        private static readonly Uri PrivoxyBaseUrl = new Uri("https://www.privoxy.org/feeds/privoxy-releases.xml");
        private static readonly Uri SourceForgeBaseUrl = new Uri("https://sourceforge.net/projects/ijbswa/rss?path=/Win32");
        private static readonly Uri PrivoxyMirrorBaseUrl = new Uri("https://www.silvester.org.uk/privoxy/Windows/");

        private readonly HttpClient _httpClient;
        private readonly TorSharpSettings _settings;

        public PrivoxyFetcher(TorSharpSettings settings, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _settings = settings;
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
                GetLatestOrNullFromRssAsync(PrivoxyBaseUrl, cts.Token, TitleStartWithWin32),
                GetLatestOrNullFromRssAsync(SourceForgeBaseUrl, cts.Token),
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

        private async Task<DownloadableFile> GetLatestOrNullFromFileListingAsync(
            Uri baseUrl,
            CancellationToken token)
        {
            await Task.Yield();

            return await FetcherHelpers.GetLatestDownloadableFileAsync(
                _httpClient,
                baseUrl,
                FileNamePattern,
                token).ConfigureAwait(false);
        }

        private async Task<DownloadableFile> GetLatestOrNullFromRssAsync(
            Uri baseUrl,
            CancellationToken token,
            Func<SyndicationItem, bool> predicate = null)
        {
            await Task.Yield();

            SyndicationFeed syndicationFeed;
            using (var response = await _httpClient.GetAsync(baseUrl, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var streamReader = new StreamReader(stream);
                    var xmlReader = XmlReader.Create(streamReader);
                    syndicationFeed = SyndicationFeed.Load(xmlReader, baseUrl);
                    if (syndicationFeed == null)
                    {
                        return null;
                    }
                }
            }

            return syndicationFeed
                .Items
                .Where(i => i.Links.Any())
                .Where(predicate ?? (i => true))
                .Where(TitleEndsWithPrivoxyZip)
                .Select(GetDownloadableFile)
                .Where(x => x != null)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();
        }

        private DownloadableFile GetDownloadableFile(SyndicationItem match)
        {
            var fileName = match.Title.Text.Split('/').Last();
            string withoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string version = withoutExtension.Substring(ToolUtility.PrivoxySettings.Prefix.Length);
            if (!Version.TryParse(version, out var parsedVersion))
            {
                return null;
            }

            var downloadUrl = match.Links.First().Uri;
            return new DownloadableFile(parsedVersion, downloadUrl);
        }

        private static bool TitleStartWithWin32(SyndicationItem i)
        {
            return i.Title.Text.StartsWith("Win32/", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TitleEndsWithPrivoxyZip(SyndicationItem item)
        {
            return Regex.IsMatch(item.Title.Text, FileNamePattern, RegexOptions.IgnoreCase);
        }

        private class SyndicationFeed
        {
            public SyndicationFeed(IReadOnlyList<SyndicationItem> items)
            {
                Items = items;
            }

            public IReadOnlyList<SyndicationItem> Items { get; }

            public static SyndicationFeed Load(XmlReader reader, Uri url)
            {
                var doc = XDocument.Load(reader);

                Guard(doc.Root.Name.LocalName == "rss", $"The root element should be 'rss', in {url.AbsoluteUri}");
                var rss = doc.Root;
                var channel = GetElement(rss, "channel", url);

                var syndicationItems = new List<SyndicationItem>();
                foreach (var item in channel.Elements("item"))
                {
                    var title = GetElement(item, "title", url);
                    var link = GetElement(item, "link", url);

                    Guard(
                        Uri.TryCreate(link.Value, UriKind.Absolute, out var itemUrl),
                        $"The link value was not an absolute URL at path {GetDisplayPath(item)}, in {url.AbsoluteUri}.");

                    syndicationItems.Add(new SyndicationItem(
                        new SyndicationItemTitle(title.Value),
                        new[] { new SyndicationItemLink(itemUrl) }));
                }

                return new SyndicationFeed(syndicationItems);
            }

            private static XElement GetElement(XElement parent, string name, Uri url)
            {
                var el = parent.Element(name);
                Guard(el != null, $"No '{name}' element was found at path {GetDisplayPath(parent)}, in {url.AbsoluteUri}.");
                return el;
            }

            private static void Guard(bool value, string message)
            {
                if (!value)
                {
                    throw new TorSharpException(message);
                }
            }

            private static string GetDisplayPath(XElement node)
            {
                var output = node.Name.ToString();
                var current = node;
                while (current.Parent != null)
                {
                    current = current.Parent;
                    output = current.Name + " -> " + output;
                }

                return output;
            }
        }

        private class SyndicationItem
        {
            public SyndicationItem(SyndicationItemTitle title, IReadOnlyList<SyndicationItemLink> links)
            {
                Title = title;
                Links = links;
            }

            public SyndicationItemTitle Title { get; }
            public IReadOnlyList<SyndicationItemLink> Links { get; }
        }

        public class SyndicationItemTitle
        {
            public SyndicationItemTitle(string text)
            {
                Text = text;
            }

            public string Text { get; }
        }

        public class SyndicationItemLink
        {
            public SyndicationItemLink(Uri uri)
            {
                Uri = uri;
            }

            public Uri Uri { get; }
        }
    }
}
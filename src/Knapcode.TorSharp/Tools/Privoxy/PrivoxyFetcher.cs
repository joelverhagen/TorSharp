using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Knapcode.TorSharp.Tools.Privoxy
{
    public class PrivoxyFetcher : IFileFetcher
    {
        private static readonly Uri BaseUrl = new Uri("http://sourceforge.net/projects/ijbswa/rss?path=/Win32");
        private readonly HttpClient _httpClient;

        public PrivoxyFetcher(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DownloadableFile> GetLatestAsync()
        {
            XDocument document;
            using (var stream = await _httpClient.GetStreamAsync(BaseUrl).ConfigureAwait(false))
            {
                var streamReader = new StreamReader(stream);
                var xmlReader = XmlReader.Create(streamReader, new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true
                });
                document = XDocument.Load(xmlReader, LoadOptions.None);
            }

            var item = document
                .Root
                .Elements()
                .Where(e => e.Name.LocalName == "channel")
                .SelectMany(e => e.Elements())
                .Where(e => e.Name.LocalName == "item")
                .Select(e => GetItem(e))
                .Where(e => e != null && IsMatch(e))
                .OrderByDescending(e => e.Published)
                .FirstOrDefault();
            
            if (item == null)
            {
                throw new TorSharpException($"No version of Privoxy could be found on RSS feed {BaseUrl}.");
            }

            var name = item.Title.Split('/').Last();
            return new DownloadableFile
            {
                Name = name,
                GetContentAsync = () => _httpClient.GetStreamAsync(item.Link)
            };
        }

        private Item GetItem(XElement el)
        {
            var titleEl = el.Elements().Where(e => e.Name.LocalName == "title").FirstOrDefault();
            var linkEl = el.Elements().Where(e => e.Name.LocalName == "link").FirstOrDefault();
            var publishedEl = el.Elements().Where(e => e.Name.LocalName == "pubDate").FirstOrDefault();

            if (titleEl == null || linkEl == null || publishedEl == null)
            {
                return null;
            }

            var title = titleEl.Value.Trim();
            var link = linkEl.Value.Trim();
            var unparsedPublished = publishedEl.Value.Trim();
            var parsedPublished = ParseRssDateTimeOffset(unparsedPublished);

            return new Item
            {
                Title = title,
                Link = link,
                Published = parsedPublished,
            };
        }

        private bool IsMatch(Item item)
        {
            return Regex.IsMatch(item.Title, @"privoxy-[\d\.]+.zip$", RegexOptions.IgnoreCase);
        }

        private static DateTimeOffset ParseRssDateTimeOffset(string input)
        {
            var readyToParse = input.Trim();

            if (readyToParse.EndsWith(" UT"))
            {
                readyToParse = readyToParse.Substring(0, readyToParse.Length - 3) + " -00:00";
            }

            return DateTimeOffset.ParseExact(readyToParse, "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);
        }

        private class Item
        {
            public string Title { get; set; }
            public string Link { get; set; }
            public DateTimeOffset Published { get; set; }
        }
    }
}
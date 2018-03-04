using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    public static class FetcherHelpers
    {
        public static async Task<string> GetStringAsync(
            this HttpClient httpClient,
            Uri requestUri,
            CancellationToken token)
        {
            using (var response = await httpClient.GetAsync(requestUri, token))
            {
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task<DownloadableFile> GetLatestDownloadableFileAsync(
            HttpClient httpClient,
            Uri baseUrl,
            string fileNamePattern,
            CancellationToken token)
        {
            var versionsContent = await httpClient.GetStringAsync(baseUrl, token).ConfigureAwait(false);
            var versions = GetLinks(versionsContent)
                .Select(x => new { Link = x, Version = GetVersion(x) })
                .Where(x => x.Version != null)
                .OrderByDescending(x => x.Version);

            string relativePath = null;
            Uri listUrl = null;
            foreach (var version in versions)
            {
                listUrl = new Uri(baseUrl, version.Link);
                var listContent = await httpClient.GetStringAsync(listUrl, token).ConfigureAwait(false);
                relativePath = GetLinks(listContent).FirstOrDefault(x => Regex.IsMatch(x, fileNamePattern, RegexOptions.IgnoreCase));
                if (relativePath != null)
                {
                    break;
                }
            }

            if (relativePath == null)
            {
                return null;
            }

            var downloadUrl = new Uri(listUrl, relativePath);
            var name = relativePath.Split('/').Last();

            return new DownloadableFile
            {
                Name = name,
                GetContentAsync = () => httpClient.GetStreamAsync(downloadUrl),
            };
        }

        private static Version GetVersion(string link)
        {
            var last = Regex
                .Matches(link, @"(?<Version>\d+(?:\.\d+)+)")
                .OfType<Match>()
                .LastOrDefault();

            if (last == null)
            {
                return null;
            }

            Version version;
            if (!Version.TryParse(last.Groups["Version"].Value, out version))
            {
                return null;
            }

            return version;
        }

        private static IEnumerable<string> GetLinks(string content)
        {
            return Regex
                .Matches(content, @"<a[^>]+?href=""(?<Link>[^""]+)"">")
                .OfType<Match>()
                .Select(x => x.Groups["Link"].Value);
        }
    }
}
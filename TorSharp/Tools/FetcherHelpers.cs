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
            using (var response = await httpClient.GetAsync(requestUri, token).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public static async Task<DownloadableFile> GetLatestDownloadableFileAsync(
            HttpClient httpClient,
            Uri baseUrl,
            string fileNamePattern,
            CancellationToken token)
        {
            var versionsContent = await httpClient.GetStringAsync(baseUrl, token).ConfigureAwait(false);
            var versionInfos = GetLinks(versionsContent)
                .Select(x => new { Link = x, Version = GetVersion(x) })
                .Where(x => x.Version != null)
                .OrderByDescending(x => x.Version);

            Uri downloadUrl = null;
            foreach (var versionInfo in versionInfos)
            {
                var listUrl = new Uri(baseUrl, versionInfo.Link);
                var listContent = await httpClient.GetStringAsync(listUrl, token).ConfigureAwait(false);

                foreach (var link in GetLinks(listContent))
                {
                    var match = Regex.Match(link, fileNamePattern, RegexOptions.IgnoreCase);
                    if (!match.Success)
                    {
                        continue;
                    }

                    // Get the version from the download URL, not the version list URL.
                    if (!Version.TryParse(match.Groups["Version"].Value, out var parsedVersion))
                    {
                        continue;
                    }

                    downloadUrl = new Uri(listUrl, link);

                    return new DownloadableFile(
                        parsedVersion,
                        downloadUrl);
                }
            }

            return null;
        }

        private static Version GetVersion(string link)
        {
            var last = Regex
                .Matches(link, @"(?<Version>\d+(?:\.\d+)+)( |%20|/)")
                .OfType<Match>()
                .LastOrDefault();

            if (last == null)
            {
                return null;
            }

            if (!Version.TryParse(last.Groups["Version"].Value, out var version))
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
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Knapcode.TorSharp.Tools.Tor
{
    internal class TorFetcher : IFileFetcher
    {
        private static readonly Uri BaseUrl = new Uri("https://dist.torproject.org/torbrowser/");
        private static readonly Uri BaseArmUrl = new Uri("https://sourceforge.net/projects/tor-browser-ports/rss");
        private readonly TorSharpSettings _settings;
        private readonly HttpClient _httpClient;

        public TorFetcher(TorSharpSettings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;
        }

        public async Task<DownloadableFile> GetLatestAsync()
        {
            var fileNamePatternAndFormat = GetFileNamePatternAndFormat();
            DownloadableFile downloadableFile;

            if (_settings.Architecture.Contains("Arm"))
            {
                if (!_settings.AllowUnofficialSources)
                    throw new TorSharpException($"{nameof(_settings.AllowUnofficialSources)} == false, there is no official tor distribution for {_settings.Architecture} platform.");

                downloadableFile = await GetLatestDownloadableArmFileAsync(_httpClient,
                    BaseArmUrl,
                    fileNamePatternAndFormat,
                    CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                downloadableFile = await FetcherHelpers.GetLatestDownloadableFileAsync(
                    _httpClient,
                    BaseUrl,
                    fileNamePatternAndFormat,
                    CancellationToken.None).ConfigureAwait(false);
            }

            if (downloadableFile == null)
            {
                throw new TorSharpException(
                    $"No version of Tor could be found under base URL {BaseUrl.AbsoluteUri} with pattern " +
                    $"{fileNamePatternAndFormat.Pattern}.");
            }

            return downloadableFile;
        }

        private FileNamePatternAndFormat GetFileNamePatternAndFormat()
        {
            var pattern = default(string);
            var format = default(ZippedToolFormat);
            if (_settings.OSPlatform == TorSharpOSPlatform.Windows)
            {
                if (_settings.Architecture == TorSharpArchitecture.X86)
                {
                    pattern = @"tor-expert-bundle-(?<Version>[\d\.]+)-windows-i686\.tar\.gz$";
                }
                else if (_settings.Architecture == TorSharpArchitecture.X64)
                {
                    pattern = @"tor-expert-bundle-(?<Version>[\d\.]+)-windows-x86_64\.tar\.gz$";
                }
                format = ZippedToolFormat.TarGz;
            }
            else if (_settings.OSPlatform == TorSharpOSPlatform.Linux)
            {
                format = ZippedToolFormat.TarGz;
                if (_settings.Architecture == TorSharpArchitecture.X86)
                {
                    pattern = @"tor-expert-bundle-(?<Version>[\d\.]+)-linux-i686\.tar\.gz$";
                }
                else if (_settings.Architecture == TorSharpArchitecture.X64)
                {
                    pattern = @"tor-expert-bundle-(?<Version>[\d\.]+)-linux-x86_64\.tar\.gz$";
                }
                else if (_settings.Architecture.Contains("Arm"))
                {
                    // Version twice to skip ALSA versions - https://github.com/alsa-project
                    var arch = _settings.Architecture == TorSharpArchitecture.Arm64 ? "arm64" : "armhf";
                    pattern = @$"(?<Version>[\d\.]+)\/tor-browser-linux-{arch}-(?<Version1>[\d\.]+)_ALL\.tar\.xz$";
                    format = ZippedToolFormat.TarXz;
                }
            }

            if (pattern == null)
            {
                _settings.RejectRuntime($"fetch Tor from {BaseUrl.AbsoluteUri}");
            }

            return new FileNamePatternAndFormat(pattern, format);
        }

        public async Task<DownloadableFile> GetLatestDownloadableArmFileAsync(HttpClient httpClient,
            Uri baseUrl,
            FileNamePatternAndFormat patternAndFormat,
            CancellationToken token)
        {
            using var response = await httpClient.GetAsync(baseUrl, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var xmlReader = XmlReader.Create(streamReader);
            var syndicationFeed = SyndicationFeed.Load(xmlReader);
            if (syndicationFeed == null)
                return null;

            return syndicationFeed.Items
                .Where(i => i.Links.Any())
                .Select(i => GetDownloadableFile(patternAndFormat.Pattern, patternAndFormat.Format, i))
                .Where(i => i != null)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();
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

            var downloadUrl = item.Links[0].Uri;
            return new DownloadableFile(parsedVersion, downloadUrl, format);
        }
    }
}
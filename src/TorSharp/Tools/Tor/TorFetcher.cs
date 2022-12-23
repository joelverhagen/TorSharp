using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools.Tor
{
    internal class TorFetcher : IFileFetcher
    {
        private static readonly Uri BaseUrl = new Uri("https://dist.torproject.org/torbrowser/");
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

            var downloadableFile = await FetcherHelpers.GetLatestDownloadableFileAsync(
                _httpClient,
                BaseUrl,
                fileNamePatternAndFormat.Pattern,
                fileNamePatternAndFormat.Format,
                CancellationToken.None).ConfigureAwait(false);

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
                else if(_settings.Architecture == TorSharpArchitecture.X64)
                {
                    pattern = @"tor-expert-bundle-(?<Version>[\d\.]+)-windows-x86_64\.tar\.gz$";
                }
                format = ZippedToolFormat.TarGz;
            }
            else if (_settings.OSPlatform == TorSharpOSPlatform.Linux)
            {
                if (_settings.Architecture == TorSharpArchitecture.X86)
                {
                    pattern = @"tor-expert-bundle-(?<Version>[\d\.]+)-linux-i686\.tar\.gz$";
                }
                else if (_settings.Architecture == TorSharpArchitecture.X64)
                {
                    pattern = @"tor-expert-bundle-(?<Version>[\d\.]+)-linux-x86_64\.tar\.gz$";
                }
                format = ZippedToolFormat.TarGz;
            }

            if (pattern == null)
            {
                _settings.RejectRuntime($"fetch Tor from {BaseUrl.AbsoluteUri}");
            }

            return new FileNamePatternAndFormat(pattern, format);
        }
    }
}
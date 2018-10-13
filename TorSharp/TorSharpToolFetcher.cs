using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tools;
using Knapcode.TorSharp.Tools.Privoxy;
using Knapcode.TorSharp.Tools.Tor;

namespace Knapcode.TorSharp
{
    public interface ITorSharpToolFetcher
    {
        Task FetchAsync();
    }

    public class TorSharpToolFetcher : ITorSharpToolFetcher
    {
        private static bool SecureProtocolsEnabled = false;

        private readonly TorSharpSettings _settings;
        private readonly PrivoxyFetcher _privoxyFetcher;
        private readonly TorFetcher _torFetcher;

        public TorSharpToolFetcher(TorSharpSettings settings, HttpClient client)
        {
            _settings = settings;
            _privoxyFetcher = new PrivoxyFetcher(client);
            _torFetcher = new TorFetcher(client);   
        }

        public async Task FetchAsync()
        {
            // If configured, enable all security protocols (a.k.a SSL/TLS protocols). This is necessary, for example,
            // because SourceForge requires at least TLS 1.1 and some .NET clients don't have this enabled. To minimize
            // the change of connection failure, enabled all protocols.
            if (_settings.EnableSecurityProtocolsForFetcher && !SecureProtocolsEnabled)
            {
                var protocols = new[]
                {
                    SecurityProtocolType.Ssl3,
                    SecurityProtocolType.Tls,
                    SecurityProtocolType.Tls11,
                    SecurityProtocolType.Tls12
                };

                foreach (var protocol in protocols)
                {
                    try
                    {
                        ServicePointManager.SecurityProtocol |= protocol;
                    }
                    catch (NotSupportedException)
                    {
                        // Not much we can do if the protocol isn't supported. isn't supported.
                    }
                }

                SecureProtocolsEnabled = true;
            }

            Directory.CreateDirectory(_settings.ZippedToolsDirectory);
            await DownloadFileAsync(_privoxyFetcher).ConfigureAwait(false);
            await DownloadFileAsync(_torFetcher).ConfigureAwait(false);
        }

        private async Task DownloadFileAsync(IFileFetcher fetcher)
        {
            var file = await fetcher.GetLatestAsync().ConfigureAwait(false);
            string filePath = Path.Combine(_settings.ZippedToolsDirectory, file.Name);
            if (!File.Exists(filePath) || _settings.ReloadTools)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var contentStream = await file.GetContentAsync().ConfigureAwait(false);
                    await contentStream.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }
        }
    }
}

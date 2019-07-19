using System;
using System.IO;
using System.IO.Compression;
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

    /// <summary>
    /// Fetch the latest version of the tools from the internet.
    /// </summary>
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

        /// <summary>
        /// Downloads the latest version of the tools to the configured
        /// <see cref="TorSharpSettings.ZippedToolsDirectory"/>.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task FetchAsync()
        {
            Directory.CreateDirectory(_settings.ZippedToolsDirectory);
            await DownloadFileAsync(_privoxyFetcher, ToolUtility.PrivoxySettings).ConfigureAwait(false);
            await DownloadFileAsync(_torFetcher, ToolUtility.TorSettings).ConfigureAwait(false);
        }

        private async Task DownloadFileAsync(IFileFetcher fetcher, ToolSettings toolSettings)
        {
            if (_settings.UseExistingTools)
            {
                var tool = ToolUtility.GetLatestToolOrNull(_settings.ZippedToolsDirectory, toolSettings);
                if (tool != null)
                {
                    return;
                }
            }

            EnableSecurityProtocols();

            var file = await fetcher.GetLatestAsync().ConfigureAwait(false);
            string filePath = Path.Combine(_settings.ZippedToolsDirectory, file.Name);
            if (!File.Exists(filePath) || _settings.ReloadTools)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        var contentStream = await file.GetContentAsync().ConfigureAwait(false);
                        await contentStream.CopyToAsync(fileStream).ConfigureAwait(false);
                    }

                    try
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Open))
                        using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                        {
                            var entries = zipArchive.Entries;
                        }
                    }
                    catch (InvalidDataException ex)
                    {
                        throw new TorSharpException($"The tool downloaded from '{file.Url.AbsoluteUri}' could not be read as a ZIP file.", ex);
                    }
                }
                catch
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        // Best effort.
                    }

                    throw;
                }
            }
        }

        private void EnableSecurityProtocols()
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
                        // Not much we can do if the protocol isn't supported. Move on and try the next one.
                    }
                }

                SecureProtocolsEnabled = true;
            }
        }
    }
}

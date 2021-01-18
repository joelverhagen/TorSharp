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
        Task<ToolUpdates> CheckForUpdatesAsync();
        Task FetchAsync();
        Task FetchAsync(ToolUpdates updates);
    }

    /// <summary>
    /// Fetch the latest version of the tools from the internet.
    /// </summary>
    public class TorSharpToolFetcher : ITorSharpToolFetcher
    {
        private static bool SecureProtocolsEnabled = false;

        private readonly TorSharpSettings _settings;
        private readonly ISimpleHttpClient _simpleHttpClient;
        private readonly IProgress<DownloadProgress> _progress;
        private readonly PrivoxyFetcher _privoxyFetcher;
        private readonly TorFetcher _torFetcher;

        public TorSharpToolFetcher(TorSharpSettings settings, HttpClient client)
            : this(settings, client, new SimpleHttpClient(client), progress: null)
        {
        }

        internal TorSharpToolFetcher(
            TorSharpSettings settings,
            HttpClient client,
            ISimpleHttpClient simpleHttpClient,
            IProgress<DownloadProgress> progress)
        {
            _settings = settings;
            _simpleHttpClient = simpleHttpClient;
            _progress = progress;
            if (!_settings.PrivoxySettings.Disable)
            {
                _privoxyFetcher = new PrivoxyFetcher(settings, client);
            }
            _torFetcher = new TorFetcher(settings, client);
        }

        /// <summary>
        /// Checks for updates of the tools and returns what is found. Inspect the
        /// <see cref="ToolUpdates.HasUpdate"/> property to determine whether updates are available. This is
        /// determined by checking the the latest versions are already downloads to the
        /// <see cref="TorSharpSettings.ZippedToolsDirectory"/>.
        /// </summary>
        /// <returns>Information about whether there are tool updates.</returns>
        public async Task<ToolUpdates> CheckForUpdatesAsync()
        {
            var updates = await CheckForUpdatesAsync(allowExistingTools: false).ConfigureAwait(false);
            return new ToolUpdates(updates.Privoxy, updates.Tor);
        }

        private async Task<PartialToolUpdates> CheckForUpdatesAsync(bool allowExistingTools)
        {
            ToolUpdate privoxy;
            if (!_settings.PrivoxySettings.Disable)
            {
                privoxy = await CheckForUpdateAsync(
                    ToolUtility.GetPrivoxyToolSettings(_settings),
                    _privoxyFetcher,
                    allowExistingTools).ConfigureAwait(false);
            }
            else
            {
                privoxy = new ToolUpdate(ToolUpdateStatus.NoUpdateAvailable, new Version(), "", new DownloadableFile(new Version(), new Uri("noupdate:///"), ZippedToolFormat.Zip));
            }

            var tor = await CheckForUpdateAsync(
                ToolUtility.GetTorToolSettings(_settings),
                _torFetcher,
                allowExistingTools).ConfigureAwait(false);

            return new PartialToolUpdates
            {
                Privoxy = privoxy,
                Tor = tor,
            };
        }

        private async Task<ToolUpdate> CheckForUpdateAsync(
            ToolSettings toolSettings,
            IFileFetcher fetcher,
            bool useExistingTools)
        {
            var latestLocal = ToolUtility.GetLatestToolOrNull(_settings, toolSettings);
            if (useExistingTools && latestLocal != null)
            {
                return null;
            }

            EnableSecurityProtocols();

            var latestDownload = await fetcher.GetLatestAsync().ConfigureAwait(false);
            var fileExtension = ArchiveUtility.GetFileExtension(latestDownload.Format);
            var fileName = $"{toolSettings.Prefix}{latestDownload.Version}{fileExtension}";
            var destinationPath = Path.Combine(_settings.ZippedToolsDirectory, fileName);

            ToolUpdateStatus status;
            if (latestLocal == null)
            {
                status = ToolUpdateStatus.NoLocalVersion;
            }
            else if (!File.Exists(destinationPath))
            {
                status = ToolUpdateStatus.NewerVersionAvailable;
            }
            else
            {
                status = ToolUpdateStatus.NoUpdateAvailable;
            }

            return new ToolUpdate(
                status,
                latestLocal?.Version,
                destinationPath,
                latestDownload);
        }

        /// <summary>
        /// Downloads the latest version of the tools to the configured
        /// <see cref="TorSharpSettings.ZippedToolsDirectory"/> given the tool updates already discoved. This should
        /// be called after getting the result of <see cref="CheckForUpdatesAsync()"/>.
        /// </summary>
        /// <param name="updates">The updates to download.</param>
        /// <returns>A task.</returns>
        public async Task FetchAsync(ToolUpdates updates)
        {
            if (updates == null)
            {
                throw new ArgumentNullException(nameof(updates));
            }

            await DownloadFileAsync(updates.Privoxy).ConfigureAwait(false);
            await DownloadFileAsync(updates.Tor).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads the latest version of the tools to the configured
        /// <see cref="TorSharpSettings.ZippedToolsDirectory"/>. This method always tries to download the latest
        /// version of the tools unless <see cref="TorSharpSettings.UseExistingTools"/> is set to true.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task FetchAsync()
        {
            var updates = await CheckForUpdatesAsync(_settings.UseExistingTools).ConfigureAwait(false);

            if (updates.Privoxy != null)
            {
                await DownloadFileAsync(updates.Privoxy).ConfigureAwait(false);
            }

            if (updates.Tor != null)
            {
                await DownloadFileAsync(updates.Tor).ConfigureAwait(false);
            }
        }

        private async Task DownloadFileAsync(ToolUpdate update)
        {
            if (update.Status != ToolUpdateStatus.NoUpdateAvailable || _settings.ReloadTools)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(update.DestinationPath));

                try
                {
                    await _simpleHttpClient.DownloadToFileAsync(
                        update.LatestDownload.Url,
                        update.DestinationPath,
                        _progress).ConfigureAwait(false);

                    try
                    {
                        await ArchiveUtility.TestAsync(
                            update.LatestDownload.Format,
                            update.DestinationPath).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new TorSharpException(
                            $"The tool downloaded from '{update.LatestDownload.Url.AbsoluteUri}' could not be read as a " +
                            $"{ArchiveUtility.GetFileExtension(update.LatestDownload.Format)} file.", ex);
                    }
                }
                catch
                {
                    try
                    {
                        File.Delete(update.DestinationPath);
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

        private class PartialToolUpdates
        {
            public ToolUpdate Privoxy { get; set; }
            public ToolUpdate Tor { get; set; }
        }
    }
}

using System;
using System.IO;

namespace Knapcode.TorSharp
{
    public class TorSharpSettings
    {
        public static readonly string DefaultToolsDirectory = Path.Combine(Path.GetTempPath(), "Knapcode.TorSharp");

        public TorSharpSettings()
        {
            ReloadTools = false;
            EnableSecurityProtocolsForFetcher = true;
            ZippedToolsDirectory = Path.Combine(DefaultToolsDirectory, "ZippedTools");
            ExtractedToolsDirectory = Path.Combine(DefaultToolsDirectory, "ExtractedTools");
            ToolRunnerType = ToolRunnerType.VirtualDesktop;
            PrivoxySettings = new TorSharpPrivoxySettings();
            TorSettings = new TorSharpTorSettings();
        }

        /// <summary>
        /// If true, the tools (Tor and Privoxy) will be re-downloaded by <see cref="TorSharpToolFetcher"/> even if the
        /// latest version is already downloaded and will be re-extracted by <see cref="TorSharpProxy"/> even if the
        /// latest version is already extracted. This is disabled by default.
        /// </summary>
        public bool ReloadTools { get; set; }

        /// <summary>
        /// Enable all SSL/TLS protocol on <see cref="System.Net.ServicePointManager.SecurityProtocol"/> so that
        /// requests to fetch the tools don't fail due to protocol mismatches. This is enabled by default.
        /// </summary>
        public bool EnableSecurityProtocolsForFetcher { get; set; }

        /// <summary>
        /// The way in which tools should be run. The default is <see cref="ToolRunnerType.VirtualDesktop"/> so that
        /// the Privoxy and Tor windows are not visible.
        /// </summary>
        public ToolRunnerType ToolRunnerType { get; set; }

        /// <summary>
        /// The directory to download the zipped tools. This defaults to <c>%TEMP%\Knapcode.TorSharp\ZippedTools</c>.
        /// </summary>
        public string ZippedToolsDirectory { get; set; }

        /// <summary>
        /// The directory to extract the tools to and run them from.  This defaults to
        /// <c>%TEMP%\Knapcode.TorSharp\ExtractedTools</c>.
        /// </summary>
        public string ExtractedToolsDirectory { get; set; }

        /// <summary>
        /// Settings specific to Privoxy.
        /// </summary>
        public TorSharpPrivoxySettings PrivoxySettings { get; set; }

        /// <summary>
        /// Settings specific to Tor.
        /// </summary>
        public TorSharpTorSettings TorSettings { get; set; }

        [Obsolete("Use the " + nameof(TorSharpPrivoxySettings) + "." + nameof(TorSharpPrivoxySettings.Port) + " property instead.")]
        public int PrivoxyPort
        {
            get => PrivoxySettings?.Port ?? TorSharpPrivoxySettings.DefaultPort;
            set => EnsurePrivoxySettings().Port = value;
        }

        [Obsolete("Use the " + nameof(TorSharpTorSettings) + "." + nameof(TorSharpTorSettings.SocksPort) + " property instead.")]
        public int TorSocksPort
        {
            get => TorSettings?.SocksPort ?? TorSharpTorSettings.DefaultSocksPort;
            set => EnsureTorSettings().SocksPort = value;
        }

        [Obsolete("Use the " + nameof(TorSharpTorSettings) + "." + nameof(TorSharpTorSettings.ControlPort) + " property instead.")]
        public int TorControlPort
        {
            get => TorSettings?.ControlPort ?? TorSharpTorSettings.DefaultControlPort;
            set => EnsureTorSettings().ControlPort = value;
        }

        [Obsolete("Use the " + nameof(TorSharpTorSettings) + "." + nameof(TorSharpTorSettings.ExitNodes) + " property instead.")]
        public string TorExitNodes
        {
            get => TorSettings?.ExitNodes;
            set => EnsureTorSettings().ExitNodes = value;
        }

        [Obsolete("Use the " + nameof(TorSharpTorSettings) + "." + nameof(TorSharpTorSettings.StrictNodes) + " property instead.")]
        public bool? TorStrictNodes
        {
            get => TorSettings?.StrictNodes;
            set => EnsureTorSettings().StrictNodes = value;
        }

        [Obsolete("Use the " + nameof(TorSharpTorSettings) + "." + nameof(TorSharpTorSettings.ControlPassword) + " property instead.")]
        public string TorControlPassword
        {
            get => TorSettings?.ControlPassword;
            set => EnsureTorSettings().ControlPassword = value;
        }

        [Obsolete("Use the " + nameof(TorSharpTorSettings) + "." + nameof(TorSharpTorSettings.HashedControlPassword) + " property instead.")]
        public string HashedTorControlPassword
        {
            get => TorSettings?.HashedControlPassword;
            set => EnsureTorSettings().HashedControlPassword = value;
        }

        [Obsolete("Use the " + nameof(TorSharpTorSettings) + "." + nameof(TorSharpTorSettings.DataDirectory) + " property instead.")]
        public string TorDataDirectory
        {
            get => TorSettings?.DataDirectory;
            set => EnsureTorSettings().DataDirectory = value;
        }

        private TorSharpTorSettings EnsureTorSettings()
        {
            if (TorSettings == null)
            {
                TorSettings = new TorSharpTorSettings();
            }

            return TorSettings;
        }

        private TorSharpPrivoxySettings EnsurePrivoxySettings()
        {
            if (PrivoxySettings == null)
            {
                PrivoxySettings = new TorSharpPrivoxySettings();
            }

            return PrivoxySettings;
        }
    }
}
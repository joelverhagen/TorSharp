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
            ZippedToolsDirectory = Path.Combine(DefaultToolsDirectory, "ZippedTools");
            ExtractedToolsDirectory = Path.Combine(DefaultToolsDirectory, "ExtractedTools");
            ToolRunnerType = ToolRunnerType.VirtualDesktop;
            PrivoxySettings = new TorSharpPrivoxySettings();
            TorSettings = new TorSharpTorSettings();
        }

        public bool ReloadTools { get; set; }
        public ToolRunnerType ToolRunnerType { get; set; }
        public string ZippedToolsDirectory { get; set; }
        public string ExtractedToolsDirectory { get; set; }
        public TorSharpPrivoxySettings PrivoxySettings { get; set; }
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
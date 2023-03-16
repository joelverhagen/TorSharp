using System;
using System.IO;
using System.Text;
#if NETSTANDARD
using System.Runtime.InteropServices;
using SystemOSPlatform = System.Runtime.InteropServices.OSPlatform;
using SystemArchitecture = System.Runtime.InteropServices.Architecture;
#endif

namespace Knapcode.TorSharp
{
    /// <summary>
    /// Settings for how TorSharp behaves.
    /// </summary>
    public class TorSharpSettings
    {
        public static readonly string DefaultToolsDirectory = Path.Combine(Path.GetTempPath(), "Knapcode.TorSharp");

        private ToolRunnerType? _toolRunnerType;

        public TorSharpSettings()
        {
            ReloadTools = false;
            EnableSecurityProtocolsForFetcher = true;
            ZippedToolsDirectory = Path.Combine(DefaultToolsDirectory, "ZippedTools");
            ExtractedToolsDirectory = Path.Combine(DefaultToolsDirectory, "ExtractedTools");
            WaitForConnect = TimeSpan.FromSeconds(5);

#if NETSTANDARD
            if (RuntimeInformation.IsOSPlatform(SystemOSPlatform.Windows))
            {
                OSPlatform = TorSharpOSPlatform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(SystemOSPlatform.Linux))
            {
                OSPlatform = TorSharpOSPlatform.Linux;
            }
            else
            {
                OSPlatform = TorSharpOSPlatform.Unknown;
            }

            Architecture = RuntimeInformation.ProcessArchitecture switch
            {
                SystemArchitecture.X86 => TorSharpArchitecture.X86,
                SystemArchitecture.X64 => TorSharpArchitecture.X64,
                SystemArchitecture.Arm => TorSharpArchitecture.Arm32,
                SystemArchitecture.Arm64 => TorSharpArchitecture.Arm64,
                _ => TorSharpArchitecture.Unknown,
            };
#else
            OSPlatform = TorSharpOSPlatform.Windows;
            Architecture = Environment.Is64BitProcess ? TorSharpArchitecture.X64 : TorSharpArchitecture.X86;
#endif

            PrivoxySettings = new TorSharpPrivoxySettings();
            TorSettings = new TorSharpTorSettings();

#if NETSTANDARD
            if (OSPlatform == TorSharpOSPlatform.Linux)
            {
                // It's simply better for any linux distro to try find and use system binaries.
                PrivoxySettings.AutomaticallyFindInSystem = true;
                TorSettings.AutomaticallyFindInSystem = true;
            }
            if (Architecture.HasFlag(TorSharpArchitecture.Arm))
            {
                PrivoxySettings.Disable = true;
            }
#endif
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
        /// The way in which tools should be run. The default is <see cref="ToolRunnerType.VirtualDesktop"/> on Windows
        /// so that the Privoxy and Tor windows are not visible and <see cref="ToolRunnerType.Simple"/> for other
        /// operating systems.
        /// </summary>
        public ToolRunnerType ToolRunnerType
        {
            get
            {
                var toolRunnerType = _toolRunnerType;
                if (toolRunnerType == null)
                {
                    if (OSPlatform == TorSharpOSPlatform.Windows
                        && Environment.OSVersion.Version >= new Version(6, 2))
                    {
                        return ToolRunnerType.VirtualDesktop;
                    }
                    else
                    {
                        return ToolRunnerType.Simple;
                    }
                }
                else
                {
                    return toolRunnerType.Value;
                }
            }

            set
            {
                _toolRunnerType = value;
            }
        }

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
        /// Instead of downloading the latest version of the tools in <see cref="TorSharpToolFetcher"/>, use any
        /// existing tools already downloaded to <see cref="ZippedToolsDirectory"/>.
        /// </summary>
        public bool UseExistingTools { get; set; }

        /// <summary>
        /// What strategy to use when fetching the latest version of a tool. This behavior matters when there are
        /// multiple URLs or feeds that a checked for a single tool.
        /// </summary>
        public ToolDownloadStrategy ToolDownloadStrategy { get; set; }

        /// <summary>
        /// The operating system that TorSharp should assume it is running on. This defaults to
        /// <see cref="TorSharpOSPlatform.Windows"/> on .NET Framework and is automatically detected on .NET Core.
        /// </summary>
        public TorSharpOSPlatform OSPlatform { get; set; }

        /// <summary>
        /// The CPU architecture that TorSharp should assume it is runnong on. This is automatically detected using the
        /// architecture of the process.
        /// </summary>
        public TorSharpArchitecture Architecture { get; set; }

        /// <summary>
        /// How long to wait for each tool to accept connections while starting up. This allows for the tools to start
        /// up before completing the initialization process. Defaults to 5 seconds. This can be completely disabled by
        /// setting this property to <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public TimeSpan WaitForConnect { get; set; }

        /// <summary>
        /// When using the <see cref="ToolRunnerType"/> of <see cref="ToolRunnerType.VirtualDesktop"/>, this will be the
        /// name of the Windows virtual desktop. It must not contain backslash ("\") characters. The name is case sensitive.
        /// The name defaults to a "TorSharpDesktop-{hash of extracted tools directory}". It should not be shared with any
        /// other parallel instances of <see cref="TorSharpProxy"/>.
        /// </summary>
        public string VirtualDesktopName { get; set; }

        /// <summary>
        /// Write the output of the underlying tools (Tor, Privoxy) to this process's stdout and stderr. Defaults to true.
        /// Use the <see cref="ITorSharpProxy.OutputDataReceived"/> and <see cref="ITorSharpProxy.ErrorDataReceived"/> to
        /// listen the output yourself.
        /// </summary>
        public bool WriteToConsole { get; set; } = true;

        /// <summary>
        /// Settings specific to Privoxy.
        /// </summary>
        public TorSharpPrivoxySettings PrivoxySettings { get; set; }

        /// <summary>
        /// Settings specific to Tor.
        /// </summary>
        public TorSharpTorSettings TorSettings { get; set; }

        /// <summary>
        /// Allow non official download repository.
        /// </summary>
        public bool AllowUnofficialSources { get; set; }

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
            TorSettings ??= new TorSharpTorSettings();

            return TorSettings;
        }

        private TorSharpPrivoxySettings EnsurePrivoxySettings()
        {
            PrivoxySettings ??= new TorSharpPrivoxySettings();

            return PrivoxySettings;
        }

        internal void RejectRuntime(string action)
        {
            var message = new StringBuilder();
            message.Append($"Cannot {action} on {OSPlatform} OS and {Architecture} architecture.");
#if NETSTANDARD
            message.Append($" OS description: {RuntimeInformation.OSDescription}.");
#endif
            throw new TorSharpException(message.ToString());
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.TorSharp.Adapters;
using Knapcode.TorSharp.Tools;
using Knapcode.TorSharp.Tools.Privoxy;
using Knapcode.TorSharp.Tools.Tor;

namespace Knapcode.TorSharp
{
    public interface ITorSharpProxy : IDisposable
    {
        Task ConfigureAndStartAsync();
        Task GetNewIdentityAsync();
        void Stop();
    }

    /// <summary>
    /// The main proxy controller implementation. This class handles extracting the tools (Privoxy and Tor),
    /// configuring them, starting them, and stopping them.
    /// </summary>
    public class TorSharpProxy : ITorSharpProxy
    {
        private const string TorHostName = "localhost";

        private bool _initialized;
        private readonly TorSharpSettings _settings;
        private readonly IToolRunner _toolRunner;
        private readonly TorPasswordHasher _torPasswordHasher;
        private Tool _tor;
        private Tool _privoxy;

        /// <summary>
        /// Initializes an instance of the proxy controller.
        /// </summary>
        /// <param name="settings">The settings to dictate the proxy's behavior.</param>
        public TorSharpProxy(TorSharpSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _torPasswordHasher = new TorPasswordHasher(new RandomFactory());

            switch (settings.ToolRunnerType)
            {
                case ToolRunnerType.Simple:
                    _toolRunner = new SimpleToolRunner();
                    break;
                case ToolRunnerType.VirtualDesktop:
                    if (settings.OSPlatform != TorSharpOSPlatform.Windows)
                    {
                        settings.RejectRuntime($"use the {nameof(ToolRunnerType.VirtualDesktop)} tool runner");
                    }
                    _toolRunner = new VirtualDesktopToolRunner();
                    break;
                default:
                    throw new NotImplementedException($"The '{settings.ToolRunnerType}' tool runner is not supported.");
            }
        }

        /// <summary>
        /// Configure the tools and start them.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ConfigureAndStartAsync()
        {
            if (!_initialized)
            {
                await InitializeAsync().ConfigureAwait(false);
                _initialized = true;
            }
        }

        private async Task InitializeAsync()
        {
            _settings.ZippedToolsDirectory = GetAbsoluteCreate(_settings.ZippedToolsDirectory);
            _settings.ExtractedToolsDirectory = GetAbsoluteCreate(_settings.ExtractedToolsDirectory);

            var torToolSettings = ToolUtility.GetTorToolSettings(_settings);
            var privoxyToolSettings = ToolUtility.GetPrivoxyToolSettings(_settings);

            _tor = await ExtractAsync(torToolSettings).ConfigureAwait(false);
            _privoxy = await ExtractAsync(privoxyToolSettings).ConfigureAwait(false);

            if (_settings.TorSettings.ControlPassword != null && _settings.TorSettings.HashedControlPassword == null)
            {
                _settings.TorSettings.HashedControlPassword = _torPasswordHasher.HashPassword(_settings.TorSettings.ControlPassword);
            }

            await ConfigureAndStartAsync(_tor, new TorConfigurationDictionary()).ConfigureAwait(false);
            await ConfigureAndStartAsync(_privoxy, new PrivoxyConfigurationDictionary()).ConfigureAwait(false);

            WaitForConnect(TorHostName, _settings.TorSettings.SocksPort);
            WaitForConnect(_settings.PrivoxySettings.ListenAddress, _settings.PrivoxySettings.Port);
        }

        /// <summary>
        /// Tell Tor to clean the circuit and get a new identity.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task GetNewIdentityAsync()
        {
            using (var client = new TorControlClient())
            {
                await client.ConnectAsync(TorHostName, _settings.TorSettings.ControlPort).ConfigureAwait(false);
                await client.AuthenticateAsync(_settings.TorSettings.ControlPassword).ConfigureAwait(false);
                await client.CleanCircuitsAsync().ConfigureAwait(false);
                await client.QuitAsync().ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Stop the tools so that the proxy is no longer listening on the configured port.
        /// </summary>
        public void Stop()
        {
            if (_initialized)
            {
                _toolRunner.Stop();
                _initialized = false;
            }
        }

        private async Task<Tool> ExtractAsync(ToolSettings toolSettings)
        {
            var tool = ToolUtility.GetLatestToolOrNull(_settings, toolSettings);
            if (tool == null)
            {
                throw new TorSharpException(
                    $"No version of {toolSettings.Name} was found under {_settings.ZippedToolsDirectory}.");
            }

            await ExtractToolAsync(tool, _settings.ReloadTools).ConfigureAwait(false);

            PermissionsUtility.MakeExecutable(_settings, tool.ExecutablePath);

            return tool;
        }

        private async Task<Tool> ConfigureAndStartAsync(Tool tool, IConfigurationDictionary configurationDictionary)
        {
            var configurer = new LineByLineConfigurer(configurationDictionary, new ConfigurationFormat());
            await configurer.ApplySettings(tool, _settings).ConfigureAwait(false);
            await _toolRunner.StartAsync(tool).ConfigureAwait(false);
            return tool;
        }

        private string GetAbsoluteCreate(string directory)
        {
            string currentDirectory = Path.GetFullPath(Directory.GetCurrentDirectory());
            if (!Path.IsPathRooted(currentDirectory))
            {
                directory = Path.Combine(currentDirectory, directory);
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }

        private async Task<bool> ExtractToolAsync(Tool tool, bool reloadTool)
        {
            if (!reloadTool && File.Exists(tool.ExecutablePath))
            {
                return false;
            }

            if (Directory.Exists(tool.DirectoryPath))
            {
                Directory.Delete(tool.DirectoryPath, true);
            }

            try
            {
                await ArchiveUtility.ExtractAsync(
                    tool.Settings.ZippedToolFormat,
                    tool.ZipPath,
                    tool.DirectoryPath,
                    tool.Settings.GetEntryPath).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new TorSharpException(
                    $"Failed to extract tool '{tool.Settings.Name}'. Verify that the .zip at path '{tool.ZipPath}' " +
                    $"is valid.",
                    ex);
            }

            return true;
        }

        private bool WaitForConnect(string host, int port)
        {
            return WaitForConnect(host, port, _settings.WaitForConnect);
        }

        private static bool WaitForConnect(string host, int port, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                return false;
            }

            var sleepDuration = TimeSpan.FromMilliseconds(100);
            var stopwatch = Stopwatch.StartNew();
            var connected = false;
            do
            {
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        var task = tcpClient.ConnectAsync(host, port);
                        task.Wait(duration);
                        if (tcpClient.Connected)
                        {
                            connected = true;
                        }
                    }
                }
                catch (Exception)
                {
                    if (duration - stopwatch.Elapsed > sleepDuration)
                    {
                        Thread.Sleep(sleepDuration);
                    }
                }
            }
            while (!connected && stopwatch.Elapsed < duration);

            return connected;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}

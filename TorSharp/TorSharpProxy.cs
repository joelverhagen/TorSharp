using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
            _toolRunner =
                settings.ToolRunnerType == ToolRunnerType.VirtualDesktop
                    ? (IToolRunner)new VirtualDesktopToolRunner()
                    : new SimpleToolRunner();
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

            _tor = Extract(ToolUtility.TorSettings);
            _privoxy = Extract(ToolUtility.PrivoxySettings);

            if (_settings.TorSettings.ControlPassword != null && _settings.TorSettings.HashedControlPassword == null)
            {
                _settings.TorSettings.HashedControlPassword = _torPasswordHasher.HashPassword(_settings.TorSettings.ControlPassword);
            }

            await ConfigureAndStartAsync(_tor, new TorConfigurationDictionary(_tor.DirectoryPath)).ConfigureAwait(false);
            await ConfigureAndStartAsync(_privoxy, new PrivoxyConfigurationDictionary()).ConfigureAwait(false);
        }

        /// <summary>
        /// Tell Tor to clean the circuit and get a new identity.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task GetNewIdentityAsync()
        {
            using (var client = new TorControlClient())
            {
                await client.ConnectAsync("localhost", _settings.TorSettings.ControlPort).ConfigureAwait(false);
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

        private Tool Extract(ToolSettings toolSettings)
        {
            Tool tool = ToolUtility.GetLatestTool(
                _settings,
                toolSettings);

            ExtractTool(tool, _settings.ReloadTools);
            return tool;
        }

        private async Task<Tool> ConfigureAndStartAsync(Tool tool, IConfigurationDictionary configurationDictionary)
        {
            var configurer = new LineByLineConfigurer(configurationDictionary, new ConfigurationFormat());
            await configurer.ApplySettings(tool.ConfigurationPath, _settings).ConfigureAwait(false);
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

        private bool ExtractTool(Tool tool, bool reloadTool)
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
                ZipFile.ExtractToDirectory(tool.ZipPath, tool.DirectoryPath);
            }
            catch (InvalidDataException ex)
            {
                throw new TorSharpException($"Failed to extract tool '{tool.Name}'. Verify that the .zip at path '{tool.ZipPath}' is valid.", ex);
            }

            if (!tool.Settings.IsNested)
            {
                return true;
            }

            string[] entries = Directory.EnumerateFileSystemEntries(tool.DirectoryPath).ToArray();
            int fileCount = entries.Count(e => !File.GetAttributes(e).HasFlag(FileAttributes.Directory));
            if (fileCount > 0 || entries.Length != 1)
            {
                throw new TorSharpException($"The tool directory for {tool.Name} was expected to only contain exactly one directory.");
            }

            // move each nested file to its parent directory, accounting for a file or directory with the same name as its directory
            string nestedDirectory = entries.First();
            string nestedDirectoryName = Path.GetFileName(nestedDirectory);
            string[] nestedEntries = Directory.EnumerateFileSystemEntries(nestedDirectory).ToArray();

            string duplicate = null;
            foreach (string nestedEntry in nestedEntries)
            {
                string fileName = Path.GetFileName(nestedEntry);
                if (fileName == nestedDirectoryName)
                {
                    duplicate = nestedEntry;
                    continue;
                }

                string newLocation = Path.Combine(tool.DirectoryPath, fileName);
                Directory.Move(nestedEntry, newLocation);
            }

            if (duplicate != null)
            {
                string temporaryLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.Move(duplicate, temporaryLocation);
                Directory.Delete(nestedDirectory, false);
                Directory.Move(temporaryLocation, nestedDirectory);
            }
            else
            {
                Directory.Delete(nestedDirectory, false);
            }

            return true;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}

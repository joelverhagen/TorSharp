using System;
using System.Collections.Generic;
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

    public class TorSharpProxy : ITorSharpProxy
    {
        private static readonly ToolSettings PrivoxyToolSettings = new ToolSettings
        {
            Prefix = "privoxy-",
            ExecutablePath = "privoxy.exe",
            WorkingDirectory = ".",
            ConfigurationPath = "config.txt",
            IsNested = true,
            GetArguments = t => new[] { t.ConfigurationPath }
        };

        private static readonly ToolSettings TorToolSettings = new ToolSettings
        {
            Prefix = "tor-win32-",
            ExecutablePath = @"Tor\tor.exe",
            WorkingDirectory = @"Tor",
            ConfigurationPath = @"Data\Tor\torrc",
            IsNested = false,
            GetArguments = t => new[] { "-f", t.ConfigurationPath }
        };
        
        private bool _initialized;
        private readonly TorSharpSettings _settings;
        private readonly IToolRunner _toolRunner;
        private readonly TorPasswordHasher _torPasswordHasher;
        private Tool _tor;
        private Tool _privoxy;

        public TorSharpProxy(TorSharpSettings settings)
        {
            _settings = settings;
            _torPasswordHasher = new TorPasswordHasher(new RandomFactory());
            _toolRunner =
                settings.ToolRunnerType == ToolRunnerType.VirtualDesktop
                    ? (IToolRunner)new VirtualDesktopToolRunner()
                    : new SimpleToolRunner();
        }

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

            _tor = Extract(TorToolSettings);
            _privoxy = Extract(PrivoxyToolSettings);

            if (_settings.TorControlPassword != null && _settings.HashedTorControlPassword == null)
            {
                _settings.HashedTorControlPassword = _torPasswordHasher.HashPassword(_settings.TorControlPassword);
            }

            await ConfigureAndStartAsync(_tor, new TorConfigurationDictionary()).ConfigureAwait(false);
            await ConfigureAndStartAsync(_privoxy, new PrivoxyConfigurationDictionary()).ConfigureAwait(false);
        }

        public async Task GetNewIdentityAsync()
        {
            using (var client = new TorControlClient())
            {
                await client.ConnectAsync("localhost", _settings.TorControlPort).ConfigureAwait(false);
                await client.AuthenticateAsync(_settings.TorControlPassword).ConfigureAwait(false);
                await client.CleanCircuitsAsync().ConfigureAwait(false);
            }
        }

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
            Tool tool = GetLatestTool(toolSettings);
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

        private Tool GetLatestTool(ToolSettings toolSettings)
        {
            string[] zipPaths = Directory
                .EnumerateFiles(_settings.ZippedToolsDirectory, $"{toolSettings.Prefix}*.zip", SearchOption.TopDirectoryOnly)
                .ToArray();

            var versions = new List<Tool>();
            foreach (string zipPath in zipPaths)
            {
                string withoutExtension = Path.GetFileNameWithoutExtension(zipPath);
                string directoryPath = Path.Combine(_settings.ExtractedToolsDirectory, withoutExtension);
                string version = withoutExtension.Substring(toolSettings.Prefix.Length);
                Version parsedVersion;
                if (!Version.TryParse(version, out parsedVersion))
                {
                    continue;
                }

                versions.Add(new Tool
                {
                    Name = toolSettings.Name,
                    Settings = toolSettings,
                    ZipPath = zipPath,
                    DirectoryPath = directoryPath,
                    Version = version,
                    ExecutablePath = Path.Combine(directoryPath, toolSettings.ExecutablePath),
                    WorkingDirectory = Path.Combine(directoryPath, toolSettings.WorkingDirectory),
                    ConfigurationPath = Path.Combine(directoryPath, toolSettings.ConfigurationPath)
                });
            }

            if (versions.Count == 0)
            {
                return null;
            }

            return versions
                .OrderByDescending(t => Version.Parse(t.Version))
                .First();
        }

        /// <summary>Extracts the provided tool.</summary>
        /// <param name="tool">The tool to extract.</param>
        /// <param name="reloadTool">Whether or not to reload the tool to the disk whether or not it already exists.</param>
        /// <returns>True if the tool was extracted, false if the tool already existed.</returns>
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

            ZipFile.ExtractToDirectory(tool.ZipPath, tool.DirectoryPath);
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

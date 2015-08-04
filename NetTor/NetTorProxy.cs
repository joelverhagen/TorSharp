using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Knapcode.NetTor.Tools;
using Knapcode.NetTor.Tools.Privoxy;
using Knapcode.NetTor.Tools.Tor;

namespace Knapcode.NetTor
{
    public class NetTorProxy
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
        private readonly NetTorSettings _settings;
        private readonly VirtualDesktopRunner _virtualDesktopRunner;

        public NetTorProxy(NetTorSettings settings)
        {
            _settings = settings;
            _virtualDesktopRunner = new VirtualDesktopRunner();
        }

        public async Task StartAsync()
        {
            if (!_initialized)
            {
                await InitializeAsync();
                _initialized = true;
            }
        }

        public void Stop()
        {
            if (_initialized)
            {
                _virtualDesktopRunner.Stop();
                _initialized = false;
            }
        }

        public async Task GetNewIdentityAsync()
        {
            var client = new TorControlClient();
            await client.ConnectAsync("localhost", _settings.TorControlPort);
            await client.AuthenticateAsync(null);
            await client.CleanCircuitsAsync();
            client.Close();
        }

        private async Task InitializeAsync()
        {
            _settings.ZippedToolsDirectory = GetAbsoluteCreate(_settings.ZippedToolsDirectory);
            _settings.ExtractedToolsDirectory = GetAbsoluteCreate(_settings.ExtractedToolsDirectory);

            await ExtractAndStartAsync(TorToolSettings, new TorConfigurationDictionary());
            await ExtractAndStartAsync(PrivoxyToolSettings, new PrivoxyConfigurationDictionary());
        }

        private async Task ExtractAndStartAsync(ToolSettings toolSettings, IConfigurationDictionary configurationDictionary)
        {
            Tool tool = GetLatestTool(toolSettings);
            ExtractTool(tool, _settings.ReloadTools);
            var configurer = new LineByLineConfigurer(configurationDictionary, new ConfigurationFormat());
            await configurer.ApplySettings(tool.ConfigurationPath, _settings);
            await _virtualDesktopRunner.StartAsync(tool);
        }

        private string GetAbsoluteCreate(string directory)
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (!Path.IsPathRooted(assemblyDirectory))
            {
                directory = Path.Combine(assemblyDirectory, directory);
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
                throw new NetTorException($"The tool directory for {tool.Name} was expected to only contain exactly one directory.");
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
    }
}

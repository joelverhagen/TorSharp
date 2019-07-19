using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Knapcode.TorSharp.Tools;

namespace Knapcode.TorSharp
{
    public static class ToolUtility
    {
        public static ToolSettings PrivoxySettings => new ToolSettings
        {
            Name = "Privoxy",
            Prefix = "privoxy-",
            ExecutablePath = "privoxy.exe",
            WorkingDirectory = ".",
            ConfigurationPath = "config.txt",
            IsNested = true,
            GetArguments = t => new[] { '\"' + t.ConfigurationPath + '\"' }
        };

        public static ToolSettings TorSettings => new ToolSettings
        {
            Name = "Tor",
            Prefix = "tor-win32-",
            ExecutablePath = @"Tor\tor.exe",
            WorkingDirectory = @"Tor",
            ConfigurationPath = @"Data\Tor\torrc",
            IsNested = false,
            GetArguments = t => new[] { "-f", '\"' + t.ConfigurationPath + '\"' }
        };

        public static Tool GetLatestToolOrNull(TorSharpSettings settings, ToolSettings toolSettings)
        {
            string[] zipPaths = Directory
                .EnumerateFiles(settings.ZippedToolsDirectory, GetPattern(toolSettings), SearchOption.TopDirectoryOnly)
                .ToArray();

            var versions = new List<Tool>();
            foreach (string zipPath in zipPaths)
            {
                string withoutExtension = Path.GetFileNameWithoutExtension(zipPath);
                string directoryPath = Path.Combine(settings.ExtractedToolsDirectory, withoutExtension);
                string version = withoutExtension.Substring(toolSettings.Prefix.Length);
                if (!Version.TryParse(version, out var parsedVersion))
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

            return versions
                .OrderByDescending(t => Version.Parse(t.Version))
                .FirstOrDefault();
        }

        public static Tool GetLatestTool(TorSharpSettings settings, ToolSettings toolSettings)
        {
            var tool = GetLatestToolOrNull(settings, toolSettings);

            if (tool == null)
            {
                throw new TorSharpException(
                    $"No version of {toolSettings.Name} ({GetPattern(toolSettings)}) was found under " +
                    $"{settings.ZippedToolsDirectory}.");
            }

            return tool;
        }

        private static string GetPattern(ToolSettings toolSettings)
        {
            return $"{toolSettings.Prefix}*.zip";
        }
    }
}

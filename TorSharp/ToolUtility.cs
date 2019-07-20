using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Knapcode.TorSharp.Tools;

namespace Knapcode.TorSharp
{
    /// <summary>
    /// A support utility for reading the directory containing the tools.
    /// </summary>
    public static class ToolUtility
    {
        /// <summary>
        /// Settings expressing the shape of the Privoxy tool directory.
        /// </summary>
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

        /// <summary>
        /// Settings expressing the shape of the Tor tool directory.
        /// </summary>
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

        /// <summary>
        /// Read the <see cref="TorSharpSettings.ZippedToolsDirectory"/> and find the latest tool matching the criteria
        /// in the provided <paramref name="toolSettings"/>. If none is found return null.
        /// </summary>
        /// <param name="zippedToolsDirectory">The directory to check.</param>
        /// <param name="toolSettings">The settings for the tool.</param>
        /// <returns>The tool, or null if none was found.</returns>
        public static Tool GetLatestToolOrNull(string zippedToolsDirectory, ToolSettings toolSettings)
        {
            if (!Directory.Exists(zippedToolsDirectory))
            {
                return null;
            }

            string[] zipPaths = Directory
                .EnumerateFiles(zippedToolsDirectory, GetPattern(toolSettings), SearchOption.TopDirectoryOnly)
                .ToArray();

            var versions = new List<Tool>();
            foreach (string zipPath in zipPaths)
            {
                string withoutExtension = Path.GetFileNameWithoutExtension(zipPath);
                string directoryPath = Path.Combine(zippedToolsDirectory, withoutExtension);
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

        /// <summary>
        /// Read the <see cref="TorSharpSettings.ZippedToolsDirectory"/> and find the latest tool matching the criteria
        /// in the provided <paramref name="toolSettings"/>. If none is found throw an exception.
        /// </summary>
        /// <param name="zippedToolsDirectory">The directory to check.</param>
        /// <param name="toolSettings">The settings for the tool.</param>
        /// <returns>The tool.</returns>
        /// <exception cref="TorSharpException">Thrown if now tool is found.</exception>
        public static Tool GetLatestTool(string zippedToolsDirectory, ToolSettings toolSettings)
        {
            var tool = GetLatestToolOrNull(zippedToolsDirectory, toolSettings);

            if (tool == null)
            {
                throw new TorSharpException(
                    $"No version of {toolSettings.Name} ({GetPattern(toolSettings)}) was found under " +
                    $"{zippedToolsDirectory}.");
            }

            return tool;
        }

        private static string GetPattern(ToolSettings toolSettings)
        {
            return $"{toolSettings.Prefix}*.zip";
        }
    }
}

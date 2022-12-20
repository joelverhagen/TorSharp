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
    internal static class ToolUtility
    {
        private const string PrivoxyName = "Privoxy";
        private const string TorName = "Tor";

        public static ToolSettings GetPrivoxyToolSettings(TorSharpSettings settings)
        {
            if (settings.OSPlatform == TorSharpOSPlatform.Windows)
            {
                return new ToolSettings
                {
                    Name = PrivoxyName,
                    Prefix = "privoxy-win32-",
                    ExecutablePathOverride = settings.PrivoxySettings.ExecutablePathOverride,
                    ExecutablePath = "privoxy.exe",
                    WorkingDirectory = ".",
                    ConfigurationPath = "config.txt",
                    GetArguments = t => new[] { '\"' + t.ConfigurationPath + '\"' },
                    GetEnvironmentVariables = t => new Dictionary<string, string>(),
                    ZippedToolFormat = ZippedToolFormat.Zip,
                    GetEntryPath = e =>
                    {
                        var firstSeperatorIndex = e.IndexOfAny(new[] { '/', '\\' });
                        if (firstSeperatorIndex >= 0)
                        {
                            return e.Substring(firstSeperatorIndex + 1);
                        }

                        return e;
                    },
                };
            }
            else if (settings.OSPlatform == TorSharpOSPlatform.Linux)
            {
                var prefix = default(string);
                if (settings.Architecture == TorSharpArchitecture.X86)
                {
                    prefix = "privoxy-linux32-";
                }
                else if (settings.Architecture == TorSharpArchitecture.X64)
                {
                    prefix = "privoxy-linux64-";
                }
                else
                {
                    settings.RejectRuntime("determine Linux Privoxy prefix");
                }

                return new ToolSettings
                {
                    Name = PrivoxyName,
                    Prefix = prefix,
                    ExecutablePathOverride = settings.PrivoxySettings.ExecutablePathOverride,
                    ExecutablePath = Path.Combine("usr", "sbin", "privoxy"),
                    WorkingDirectory = Path.Combine("usr", "sbin"),
                    ConfigurationPath = Path.Combine("usr", "share", "privoxy", "config"),
                    GetArguments = t => new[] { "--no-daemon", '\"' + t.ConfigurationPath + '\"' },
                    GetEnvironmentVariables = t => new Dictionary<string, string>(),
                    ZippedToolFormat = ZippedToolFormat.Deb,
                    GetEntryPath = e =>
                    {
                        if (e.StartsWith("./usr/sbin"))
                        {
                            return e;
                        }
                        else if (e.StartsWith("./usr/share/privoxy"))
                        {
                            return e;
                        }
                        else if (e.StartsWith("./etc/privoxy"))
                        {
                            if (e.StartsWith("./etc/privoxy/templates"))
                            {
                                return null;
                            }

                            return e;
                        }
                        else
                        {
                            return null;
                        }
                    },
                };
            }
            else
            {
                settings.RejectRuntime("run Privoxy");
            }

            throw new NotImplementedException();
        }

        public static ToolSettings GetTorToolSettings(TorSharpSettings settings)
        {
            if (settings.OSPlatform == TorSharpOSPlatform.Windows)
            {
                return new ToolSettings
                {
                    Name = TorName,
                    Prefix = settings.Architecture == TorSharpArchitecture.X64 ? "tor-win64-" : "tor-win32-",
                    ExecutablePathOverride = settings.TorSettings.ExecutablePathOverride,
                    ExecutablePath = Path.Combine(TorName, "tor.exe"),
                    WorkingDirectory = TorName,
                    ConfigurationPath = Path.Combine("Data", TorName, "torrc"),
                    GetArguments = t => new[] { "-f", '\"' + t.ConfigurationPath + '\"' },
                    GetEnvironmentVariables = t => new Dictionary<string, string>(),
                    ZippedToolFormat = ZippedToolFormat.TarGz,
                    GetEntryPath = e => e,
                };
            }
            else if (settings.OSPlatform == TorSharpOSPlatform.Linux)
            {
                var prefix = default(string);
                if (settings.Architecture == TorSharpArchitecture.X86)
                {
                    prefix = "tor-linux32-";
                }
                else if (settings.Architecture == TorSharpArchitecture.X64)
                {
                    prefix = "tor-linux64-";
                }
                else
                {
                    settings.RejectRuntime("determine Linux Tor prefix");
                }

                return new ToolSettings
                {
                    Name = TorName,
                    Prefix = prefix,
                    ExecutablePathOverride = settings.TorSettings.ExecutablePathOverride,
                    ExecutablePath = Path.Combine(TorName, "tor"),
                    WorkingDirectory = TorName,
                    ConfigurationPath = Path.Combine("Data", TorName, "torrc"),
                    GetArguments = t => new[] { "-f", '\"' + t.ConfigurationPath + '\"' },
                    GetEnvironmentVariables = t =>
                    {
                        var output = new Dictionary<string, string>();

                        if (settings.TorSettings.ExecutablePathOverride == null)
                        {
                            const string ldLibraryPathKey = "LD_LIBRARY_PATH";
                            var ldLibraryPath = Environment.GetEnvironmentVariable(ldLibraryPathKey);
                            var addedPath = Path.GetDirectoryName(t.ExecutablePath);

                            if (string.IsNullOrWhiteSpace(ldLibraryPath))
                            {
                                ldLibraryPath = addedPath;
                            }
                            else
                            {
                                ldLibraryPath += ":" + addedPath;
                            }

                            output[ldLibraryPathKey] = ldLibraryPath;
                        }

                        return output;
                    },
                    ZippedToolFormat = ZippedToolFormat.TarXz,
                    GetEntryPath = e =>
                    {
                        const string entryPrefix = "tor-browser/Browser/TorBrowser/";
                        if (e.StartsWith(entryPrefix + "Data/Tor/"))
                        {
                            return e.Substring(entryPrefix.Length);
                        }
                        else if (e.StartsWith(entryPrefix + "Tor/"))
                        {
                            if (e.StartsWith(entryPrefix + "Tor/PluggableTransports/"))
                            {
                                return null;
                            }
                            else
                            {
                                return e.Substring(entryPrefix.Length);
                            }
                        }
                        else
                        {
                            return null;
                        }
                    },
                };
            }
            else
            {
                settings.RejectRuntime("run Tor");
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Read the <see cref="TorSharpSettings.ZippedToolsDirectory"/> and find the latest tool matching the criteria
        /// in the provided <paramref name="toolSettings"/>. If none is found return null.
        /// </summary>
        /// /// <param name="settings">The settings for TorSharp.</param>
        /// <param name="toolSettings">The settings for the tool.</param>
        /// <returns>The tool, or null if none was found.</returns>
        public static Tool GetLatestToolOrNull(
            TorSharpSettings settings,
            ToolSettings toolSettings)
        {
            if (!Directory.Exists(settings.ZippedToolsDirectory))
            {
                return null;
            }

            var fileExtension = ArchiveUtility.GetFileExtension(toolSettings.ZippedToolFormat);
            var pattern = $"{toolSettings.Prefix}*{fileExtension}";

            string[] zipPaths = Directory
                .EnumerateFiles(settings.ZippedToolsDirectory, pattern, SearchOption.TopDirectoryOnly)
                .ToArray();

            var versions = new List<Tool>();
            foreach (string zipPath in zipPaths)
            {
                string fileName = Path.GetFileName(zipPath);
                string withoutExtension = fileName.Substring(0, fileName.Length - fileExtension.Length);
                string directoryPath = Path.Combine(settings.ExtractedToolsDirectory, withoutExtension);
                string version = withoutExtension.Substring(toolSettings.Prefix.Length);
                if (!Version.TryParse(version, out var parsedVersion))
                {
                    continue;
                }

                versions.Add(new Tool
                {
                    Settings = toolSettings,
                    ZipPath = zipPath,
                    DirectoryPath = directoryPath,
                    Version = parsedVersion,
                    ExecutablePath = toolSettings.ExecutablePathOverride ?? Path.Combine(directoryPath, toolSettings.ExecutablePath),
                    WorkingDirectory = Path.Combine(directoryPath, toolSettings.WorkingDirectory),
                    ConfigurationPath = Path.Combine(directoryPath, toolSettings.ConfigurationPath),
                });
            }

            return versions
                .OrderByDescending(t => t.Version)
                .FirstOrDefault();
        }
    }
}

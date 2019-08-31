using System;

namespace Knapcode.TorSharp.Tools
{
    /// <summary>
    /// An extracted instance of a tool.
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// The generic settings of the tool, agnostic of this specific extracted instance.
        /// </summary>
        public ToolSettings Settings { get; set; }

        /// <summary>
        /// The path to the compressed archive that was used to initialize the tool.
        /// </summary>
        public string ZipPath { get; set; }

        /// <summary>
        /// The version of the tool.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The base extracted directory of the tool.
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// The full path to the tool's main executable.
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// The full path to the tool's working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// The full path to the tool's configuration file.
        /// </summary>
        public string ConfigurationPath { get; set; }
    }
}
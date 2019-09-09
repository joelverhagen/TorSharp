using System;
using System.Collections.Generic;

namespace Knapcode.TorSharp.Tools
{
    /// <summary>
    /// Information on how a tool is laid out on disk and how it should be interacted with by TorSharp.
    /// </summary>
    internal class ToolSettings
    {
        /// <summary>
        /// The name of the tool for messages.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The prefix of the compressed tool's file name. For example, "tor-linux32-".
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The relative path inside the extracted directory to the tool's main exectuable.
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// The relative path inside the extracted directory to use as the tool's working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// The relative path inside the extracted directory to the tool's configuration file.
        /// </summary>
        public string ConfigurationPath { get; set; }

        /// <summary>
        /// The delegate to get the arguments for a tool, given a tool instance.
        /// </summary>
        public Func<Tool, IEnumerable<string>> GetArguments { get; set; }

        /// <summary>
        /// The delegate to get the additional environment variables for a tool, given a tool instance.
        /// </summary>
        public Func<Tool, Dictionary<string, string>> GetEnvironmentVariables { get; set; }

        /// <summary>
        /// The format of the compress tool archive file.
        /// </summary>
        public ZippedToolFormat ZippedToolFormat { get; set; }

        /// <summary>
        /// A delegate to determine whether an entry (file) in the tool archive should be extracted. The input string
        /// is the entry name inside the archive. The returned string is the relative path inside the extracted
        /// directory to use for the entry. If null is returned, the entry is skipped.
        /// </summary>
        public Func<string, string> GetEntryPath { get; set; }
    }
}
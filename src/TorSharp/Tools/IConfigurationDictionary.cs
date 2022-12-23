using System.Collections.Generic;

namespace Knapcode.TorSharp.Tools
{
    /// <summary>
    /// An interface for building tool configuration.
    /// </summary>
    internal interface IConfigurationDictionary
    {
        /// <summary>
        /// Configure a tool given the provided settings.
        /// </summary>
        /// <param name="tool">The tool that is being configured.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The configuration key and values to be written to a file.</returns>
        IDictionary<string, List<string>> GetDictionary(Tool tool, TorSharpSettings settings);
    }
}
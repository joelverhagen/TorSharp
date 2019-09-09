using System;

namespace Knapcode.TorSharp
{
    /// <summary>
    /// Update information for the tools needed to run <see cref="TorSharpProxy"/>.
    /// </summary>
    public class ToolUpdates
    {
        public ToolUpdates(ToolUpdate privoxy, ToolUpdate tor)
        {
            Privoxy = privoxy ?? throw new ArgumentNullException(nameof(privoxy));
            Tor = tor ?? throw new ArgumentNullException(nameof(tor));
        }

        /// <summary>
        /// Whether or not there is an update available for one or more tools.
        /// </summary>
        public bool HasUpdate => Privoxy.HasUpdate || Tor.HasUpdate;

        /// <summary>
        /// Update information for Privoxy.
        /// </summary>
        public ToolUpdate Privoxy { get; }

        /// <summary>
        /// Update information for Tor.
        /// </summary>
        public ToolUpdate Tor { get; }
    }
}

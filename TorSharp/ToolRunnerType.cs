namespace Knapcode.TorSharp
{
    /// <summary>
    /// The types of tools runner.
    /// </summary>
    public enum ToolRunnerType
    {
        /// <summary>
        /// A tool runner that uses the Windows API to create a virtual desktop and process jobs
        /// to start a tool and keep it hidden. The associated implementation of
        /// <see cref="IToolRunner"/> is <see cref="VirtualDesktopToolRunner"/>.
        /// </summary>
        VirtualDesktop,

        /// <summary>
        /// A tool runner that uses basic processes to start and stop jobs. The associated
        /// implementation of <see cref="IToolRunner"/> is <see cref="SimpleToolRunner"/>.
        /// </summary>
        Simple
    }
}
namespace Knapcode.TorSharp
{
    /// <summary>
    /// The status of the tool update.
    /// </summary>
    public enum ToolUpdateStatus
    {
        /// <summary>
        /// No update is available meaning the latest local tool is the same version as the version available remotely.
        /// </summary>
        NoUpdateAvailable,

        /// <summary>
        /// There is no version available locally. In this case, <see cref="TorSharpProxy"/> will fail to run if no
        /// tool is downloaded and put into the <see cref="TorSharpSettings.ZippedToolsDirectory"/>.
        /// </summary>
        NoLocalVersion,

        /// <summary>
        /// A version is available locally but a newer version is available remotely. In this case,
        /// <see cref="TorSharpProxy"/> will run using this existing local version if no newer version is downloaded.
        /// </summary>
        NewerVersionAvailable,
    }
}

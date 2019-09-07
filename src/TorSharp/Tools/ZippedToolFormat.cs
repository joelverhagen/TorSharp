namespace Knapcode.TorSharp.Tools
{
    /// <summary>
    /// The format of a zipped tool, i.e. how it is compressed.
    /// </summary>
    public enum ZippedToolFormat
    {
        /// <summary>
        /// A ZIP file.
        /// </summary>
        Zip,

        /// <summary>
        /// A Debian package.
        /// </summary>
        Deb,

        /// <summary>
        /// A tarball file compressed using the XZ compression format.
        /// </summary>
        TarXz,
    }
}
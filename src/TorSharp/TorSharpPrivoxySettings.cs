namespace Knapcode.TorSharp
{
    public class TorSharpPrivoxySettings
    {
        internal const int DefaultPort = 18118;

        public TorSharpPrivoxySettings()
        {
            Port = DefaultPort;
        }

        /// <summary>
        /// The port that Privoxy listens to. This is the port of the HTTP proxy. This defaults to 18118.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The address that Privoxy listens to. This defaults to 127.0.0.1.
        /// </summary>
        public string ListenAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// Override that path to the Privoxy executable. The simplest usage is to ensure "privoxy" is in the PATH
        /// environment variable and setting this property to "privoxy". This setting is intended for providing an
        /// executable already installed to the host machine rather than the one extracted from the tools archive.
        /// Note that this does not disable the tool fetching or extraction process since the base configuration and
        /// other peripheral files still are extractied from the <see cref="TorSharpSettings.ZippedToolsDirectory"/>.
        /// This is helpful when the tool fetched via the <see cref="TorSharpToolFetcher"/> is not compatible with the
        /// host machine. 
        /// </summary>
        public string ExecutablePathOverride { get; set; }
    }
}
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
        /// This defaults to false. Disables the fetching and starting of the Privoxy tool. This should only be enabled
        /// if you have an alternate way to proxy HTTP traffic to Tor (e.g. an independent Privoxy instance or the
        /// built-in SOCKS5 support added to .NET 6: https://devblogs.microsoft.com/dotnet/dotnet-6-networking-improvements/#socks-proxy-support).
        /// </summary>
        public bool Disable { get; set; }

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

        /// <summary>
        /// Maximum number of client connections that will be served.
        /// See https://www.privoxy.org/user-manual/config.html#MAX-CLIENT-CONNECTIONS for more details.
        /// </summary>
        public int? MaxClientConnections { get; set; }

        /// <summary>
        /// Automate find privoxy in system. Must be helpful for linux users when you can install privoxy from system repositorys.
        /// </summary>
        public bool AutomateFindInSystem { get; set; }
    }
}
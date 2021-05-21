using System.Collections.Generic;

namespace Knapcode.TorSharp
{
    /// <summary>
    /// Settings specific to Tor.
    /// </summary>
    public class TorSharpTorSettings
    {
        internal const int DefaultSocksPort = 19050;
        internal const int DefaultControlPort = 19051;

        /// <summary>
        /// Initializes an instance of the TorSharp settings with default ports set.
        /// </summary>
        public TorSharpTorSettings()
        {
            SocksPort = DefaultSocksPort;
            ControlPort = DefaultControlPort;
            AdditionalSockPorts = new List<int>();
        }

        /// <summary>
        /// The port that Tor listens to for SOCKS. This is the port that Privoxy tunnels to. This defaults to 19050.
        /// See https://www.torproject.org/docs/tor-manual.html.en#SocksPort for details.
        /// </summary>
        public int SocksPort { get; set; }

        /// <summary>
        /// The port that Tor listens to for HTTP, to use Tor directly as an HTTP proxy, instead of using Privoxy.
        /// See https://2019.www.torproject.org/docs/tor-manual.html.en#HTTPTunnelPort for details.
        /// </summary>
        public int HttpTunnelPort { get; set; }

        /// <summary>
        /// Additional ports that Tor listens to for SOCKS. These ports are not used by Privoxy but can be used for
        /// other purposes. Defaults to an empty list.
        /// See https://www.torproject.org/docs/tor-manual.html.en#SocksPort for details.
        /// </summary>
        public List<int> AdditionalSockPorts { get; set; }

        /// <summary>
        /// The port that Tor listens to for control and administrative commands. This defaults to 19051.
        /// See https://www.torproject.org/docs/tor-manual.html.en#ControlPort for details.
        /// </summary>
        public int ControlPort { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual.html.en#ExitNodes for details.
        /// </summary>
        public string ExitNodes { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual.html.en#StrictNodes for details.
        /// </summary>
        public bool? StrictNodes { get; set; }

        /// <summary>
        /// The unhashed version of <see cref="HashedControlPassword"/>. TorSharp will hash it on your behalf if set and
        /// <see cref="HashedControlPassword"/> is not set.
        /// </summary>
        public string ControlPassword { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual.html.en#HashedControlPassword for more details.
        /// </summary>
        public string HashedControlPassword { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual.html.en#DataDirectory for more details.
        /// </summary>
        public string DataDirectory { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual.html.en#HTTPSProxy for more details.
        /// </summary>
        public string HttpsProxyHost { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual.html.en#HTTPSProxy for more details.
        /// </summary>
        public int? HttpsProxyPort { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual.html.en#HTTPSProxyAuthenticator for more details.
        /// </summary>
        public string HttpsProxyUsername { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual.html.en#HTTPSProxyAuthenticator for more details.
        /// </summary>
        public string HttpsProxyPassword { get; set; }

        /// <summary>
        /// Override that path to the Tor executable. The simplest usage is to ensure "tor" is in the PATH
        /// environment variable and setting this property to "tor". This setting is intended for providing an
        /// executable already installed to the host machine rather than the one extracted from the tools archive.
        /// Note that this does not disable the tool fetching or extraction process since the base configuration and
        /// other peripheral files still are extractied from the <see cref="TorSharpSettings.ZippedToolsDirectory"/>.
        /// This is helpful when the tool fetched via the <see cref="TorSharpToolFetcher"/> is not compatible with the
        /// host machine. 
        /// </summary>
        public string ExecutablePathOverride { get; set; }

        /// <summary>
        /// See https://www.torproject.org/docs/tor-manual-dev.html.en#UseBridges for more details.
        /// </summary>
        public bool? UseBridges { get; set; }

        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#ClientTransportPlugin for more details.
        /// Path to client transport plugin such as obfs4proxy, snowflake or other.
        /// ex. [transport] exec [Path]
        /// </summary>
        public string ClientTransportPlugin { get; set; }

        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#Bridge for more details.
        /// Transport plugin bridge.
        /// ex. bridge [transport] ip:port [fingerprint] key1=val2 key2=val2 keyN=valN...
        /// </summary>
        public string Bridge { get; set; }
        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#Nickname for more details.
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#ContactInfo for more details.
        /// </summary>
        public string ContactInfo { get; set; }
        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#ServerTransportListenAddr for more details.
        /// </summary>
        public string ServerTransportListenAddr { get; set; }
        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#ExitPolicy for more details.
        /// </summary>
        public string ExitPolicy { get; set; }
        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#PublishServerDescriptor for more details.
        /// </summary>
        public string PublishServerDescriptor { get; set; }
        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#BridgeRelay for more details.
        /// </summary>
        public bool? BridgeRelay { get; set; }
        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#ExtORPort for more details.
        /// </summary>
        public string ExtORPort { get; set; }
        /// <summary>
        /// See https://2019.www.torproject.org/docs/tor-manual-dev.html.en#ORPort for more details.
        /// </summary>
        public string ORPort { get; set; }
    }
}
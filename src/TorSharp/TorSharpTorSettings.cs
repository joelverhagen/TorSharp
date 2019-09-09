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
        }

        /// <summary>
        /// The port that Tor listens to for SOCKS. This is the port that Privoxy tunnels to. This defaults to 19050.
        /// See https://www.torproject.org/docs/tor-manual.html.en#SocksPort for details.
        /// </summary>
        public int SocksPort { get; set; }

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
    }
}
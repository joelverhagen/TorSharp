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
    }
}
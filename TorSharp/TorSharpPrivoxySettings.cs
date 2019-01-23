namespace Knapcode.TorSharp
{
    public class TorSharpPrivoxySettings
    {
        internal const int DefaultPort = 18118;

        public TorSharpPrivoxySettings()
        {
            Port = DefaultPort;
        }

        public int Port { get; set; }

        public string ListenAddress { get; set; } = "127.0.0.1";
    }
}
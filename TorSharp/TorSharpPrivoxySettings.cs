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
    }
}
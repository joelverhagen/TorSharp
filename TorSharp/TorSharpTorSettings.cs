namespace Knapcode.TorSharp
{
    public class TorSharpTorSettings
    {
        internal const int DefaultSocksPort = 19050;
        internal const int DefaultControlPort = 19051;

        public TorSharpTorSettings()
        {
            SocksPort = DefaultSocksPort;
            ControlPort = DefaultControlPort;
        }

        public int SocksPort { get; set; }
        public int ControlPort { get; set; }
        public string ExitNodes { get; set; }
        public bool? StrictNodes { get; set; }
        public string ControlPassword { get; set; }
        public string HashedControlPassword { get; set; }
        public string DataDirectory { get; set; }
    }
}
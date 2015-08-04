namespace Knapcode.TorSharp
{
    public class TorSharpSettings
    {
        public bool ReloadTools { get; set; }
        public string ZippedToolsDirectory { get; set; }
        public string ExtractedToolsDirectory { get; set; }
        public int TorSocksPort { get; set; }
        public int TorControlPort { get; set; }
        public int PrivoxyPort { get; set; }
        public string TorControlPassword { get; set; }
        public string HashedTorControlPassword { get; set; }
    }
}
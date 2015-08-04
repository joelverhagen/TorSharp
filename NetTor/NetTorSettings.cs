namespace Knapcode.NetTor
{
    public class NetTorSettings
    {
        public bool ReloadTools { get; set; }
        public string ZippedToolsDirectory { get; set; }
        public string ExtractedToolsDirectory { get; set; }
        public int TorSocksPort { get; set; }
        public int TorControlPort { get; set; }
        public int PrivoxyPort { get; set; }
    }
}
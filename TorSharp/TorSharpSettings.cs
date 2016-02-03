using System.IO;

namespace Knapcode.TorSharp
{
    public class TorSharpSettings
    {
        public static readonly string DefaultToolsDirectory = Path.Combine(Path.GetTempPath(), "Knapcode.TorSharp");

        public TorSharpSettings()
        {
            ReloadTools = false;
            ZippedToolsDirectory = Path.Combine(DefaultToolsDirectory, "ZippedTools");
            ExtractedToolsDirectory = Path.Combine(DefaultToolsDirectory, "ExtractedTools");
            PrivoxyPort = 18118;
            TorSocksPort = 19050;
            TorControlPort = 19051;
        }

        public bool ReloadTools { get; set; }
        public string ZippedToolsDirectory { get; set; }
        public string ExtractedToolsDirectory { get; set; }
        public int TorSocksPort { get; set; }
        public int TorControlPort { get; set; }
        public int PrivoxyPort { get; set; }
        public string TorControlPassword { get; set; }
        public string HashedTorControlPassword { get; set; }
        public string TorDataDirectory { get; set; }
    }
}
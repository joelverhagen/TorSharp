namespace Knapcode.NetTor.Tools
{
    public class Tool
    {
        public ToolSettings Settings { get; set; }
        public string Name { get; set; }
        public string ZipPath { get; set; }
        public string Version { get; set; }
        public string DirectoryPath { get; set; }
        public string ExecutablePath { get; set; }
        public string WorkingDirectory { get; set; }
        public string ConfigurationPath { get; set; }
    }
}
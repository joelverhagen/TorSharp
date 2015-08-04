using System;
using System.Collections.Generic;

namespace Knapcode.TorSharp.Tools
{
    public class ToolSettings
    {
        public string Name { get; set; }
        public string Prefix { get; set; }
        public bool IsNested { get; set; }
        public string ExecutablePath { get; set; }
        public string WorkingDirectory { get; set; }
        public string ConfigurationPath { get; set; }
        public Func<Tool, IEnumerable<string>> GetArguments { get; set; }
    }
}
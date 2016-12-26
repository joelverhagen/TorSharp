using System;
using System.IO;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    public class DownloadableFile
    {
        public Func<Task<Stream>> GetContentAsync { get; set; }
        public string Name { get; set; }
    }
}
namespace Knapcode.TorSharp.Tools
{
    internal class FileNamePatternAndFormat
    {
        public FileNamePatternAndFormat(string pattern, ZippedToolFormat format)
        {
            Pattern = pattern;
            Format = format;
        }

        public string Pattern { get; }
        public ZippedToolFormat Format { get; }
    }
}
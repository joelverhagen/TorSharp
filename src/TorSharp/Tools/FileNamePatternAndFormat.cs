namespace Knapcode.TorSharp.Tools
{
    /// <summary>
    /// The file name and pattern to be used for matching a fetched (downloaded) tool like Tor or Privoxy.
    /// </summary>
    public class FileNamePatternAndFormat
    {
        public FileNamePatternAndFormat(string pattern, ZippedToolFormat format)
        {
            Pattern = pattern;
            Format = format;
        }

        /// <summary>
        /// The regex pattern to apply to a candidate file name to determine if it should match.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// The archive format of the matched tool.
        /// </summary>
        public ZippedToolFormat Format { get; }
    }
}
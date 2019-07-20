using System;

namespace Knapcode.TorSharp.Tools
{
    public class DownloadableFile
    {
        public DownloadableFile(Version version, Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (!url.IsAbsoluteUri)
            {
                throw new ArgumentException("The URL must be absolute.", nameof(url));
            }

            Version = version ?? throw new ArgumentNullException(nameof(version));
            Url = url;
        }

        /// <summary>
        /// The version of the tool to download.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The absolute URL of the tool download.
        /// </summary>
        public Uri Url { get; }
    }
}
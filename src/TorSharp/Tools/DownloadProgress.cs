using System;

namespace Knapcode.TorSharp.Tools
{
    internal struct DownloadProgress
    {
        public DownloadProgress(Uri requestUri, long complete, long? total)
        {
            RequestUri = requestUri;
            Complete = complete;
            Total = total;
        }

        public Uri RequestUri { get; }
        public long Complete { get; }
        public long? Total { get; }
    }
}

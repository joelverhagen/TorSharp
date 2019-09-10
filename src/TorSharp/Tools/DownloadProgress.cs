using System;

namespace Knapcode.TorSharp.Tools
{
    internal struct DownloadProgress
    {
        public DownloadProgress(Guid downloadId, DownloadProgressState state, Uri requestUri, long totalRead, long? contentLength)
        {
            DownloadId = downloadId;
            State = state;
            RequestUri = requestUri;
            TotalRead = totalRead;
            ContentLength = contentLength;
        }

        public Guid DownloadId { get; }
        public DownloadProgressState State { get; }
        public Uri RequestUri { get; }
        public long TotalRead { get; }
        public long? ContentLength { get; }
    }

    internal enum DownloadProgressState
    {
        Starting,
        Progress,
        Complete,
    }
}

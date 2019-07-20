using Knapcode.TorSharp.Tools;

namespace Knapcode.TorSharp
{
    public class ToolUpdate
    {
        public ToolUpdate(
            ToolUpdateStatus status,
            string destinationPath,
            DownloadableFile latestDownload)
        {
            Status = status;
            DestinationPath = destinationPath;
            LatestDownload = latestDownload;
        }

        /// <summary>
        /// Whether or not there is an update available for this tool.
        /// </summary>
        public bool HasUpdate
        {
            get
            {
                return Status == ToolUpdateStatus.NoLocalVersion
                    || Status == ToolUpdateStatus.NewerVersionAvailable;
            }
        }

        public ToolUpdateStatus Status { get; }

        /// <summary>
        /// The path that <see cref="LatestDownload"/> would be downloaded to by <see cref="TorSharpToolFetcher"/>.
        /// </summary>
        public string DestinationPath { get; }

        /// <summary>
        /// Information about the latest downloadable tool.
        /// </summary>
        public DownloadableFile LatestDownload { get; }
    }
}

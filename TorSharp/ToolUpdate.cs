using System;
using Knapcode.TorSharp.Tools;

namespace Knapcode.TorSharp
{
    public class ToolUpdate
    {
        public ToolUpdate(
            ToolUpdateStatus status,
            Version localVersion,
            string destinationPath,
            DownloadableFile latestDownload)
        {
            Status = status;
            LocalVersion = localVersion;
            DestinationPath = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            LatestDownload = latestDownload ?? throw new ArgumentNullException(nameof(latestDownload));
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
        /// The latest local version. Null if no local version exists as indicated by <see cref="Status"/> with a value
        /// of <see cref="ToolUpdateStatus.NoLocalVersion"/>.
        /// </summary>
        public Version LocalVersion { get; }

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

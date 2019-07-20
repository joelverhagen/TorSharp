namespace Knapcode.TorSharp
{
    public enum ToolDownloadStrategy
    {
        /// <summary>
        /// Download the first version that comes back and cancel the other requests.
        /// </summary>
        First,

        /// <summary>
        /// Download the latest version. Requests that fail are ignored but at least one request must succeed.
        /// </summary>
        Latest,
    }
}
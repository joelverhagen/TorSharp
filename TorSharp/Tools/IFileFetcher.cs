using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    internal interface IFileFetcher
    {
        Task<DownloadableFile> GetLatestAsync();
    }
}
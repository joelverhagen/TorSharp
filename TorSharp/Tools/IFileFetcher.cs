using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    public interface IFileFetcher
    {
        Task<DownloadableFile> GetLatestAsync();
    }
}
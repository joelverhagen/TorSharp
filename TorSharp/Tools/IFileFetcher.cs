using System.Threading.Tasks;
using Knapcode.TorSharp.Tools.Privoxy;

namespace Knapcode.TorSharp.Tools
{
    public interface IFileFetcher
    {
        Task<DownloadableFile> GetLatestAsync();
    }
}
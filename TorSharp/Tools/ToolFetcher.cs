using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tools.Privoxy;
using Knapcode.TorSharp.Tools.Tor;

namespace Knapcode.TorSharp.Tools
{
    public class ToolFetcher
    {
        private readonly TorSharpSettings _settings;
        private readonly PrivoxyFetcher _privoxyFetcher;
        private readonly TorFetcher _torFetcher;

        public ToolFetcher(TorSharpSettings settings, HttpClient client)
        {
            _settings = settings;
            _privoxyFetcher = new PrivoxyFetcher(client);
            _torFetcher = new TorFetcher(client);   
        }

        public async Task FetchAsync()
        {
            Directory.CreateDirectory(_settings.ZippedToolsDirectory);
            await DownloadFileAsync(_privoxyFetcher);
            await DownloadFileAsync(_torFetcher);
        }

        private async Task DownloadFileAsync(IFileFetcher fetcher)
        {
            var file = await fetcher.GetLatestAsync();
            string filePath = Path.Combine(_settings.ZippedToolsDirectory, file.Name);
            if (!File.Exists(filePath) || _settings.ReloadTools)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var contentStream = await file.GetContentAsync();
                    await contentStream.CopyToAsync(fileStream);
                }
            }
        }
    }
}

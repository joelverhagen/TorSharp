using System.Net;
using Knapcode.TorSharp;

// Share the same downloaded tools with all instances.
var baseSettings = new TorSharpSettings();
using (var httpClient = new HttpClient())
{
    var fetcher = new TorSharpToolFetcher(baseSettings, httpClient);
    await fetcher.FetchAsync();
}

// Start the parallel instances with a barrier to ensure some parallelism.
var parallelInstances = 4;
var barrier = new Barrier(parallelInstances);
var tasks = Enumerable
    .Range(0, parallelInstances)
    .Select(i => RunInstanceAsync(baseSettings, i.ToString(), 10000 + (i * 100), barrier))
    .ToList();
await Task.WhenAny(tasks);
await Task.WhenAll(tasks);

async Task RunInstanceAsync(TorSharpSettings baseSettings, string name, int startingPort, Barrier barrier)
{
    var settings = new TorSharpSettings
    {
        // The extracted tools directory must not be shared.
        ExtractedToolsDirectory = Path.Combine(baseSettings.ExtractedToolsDirectory, name),

        // The zipped tools directory can be shared, as long as the tool fetcher does not run in parallel.
        ZippedToolsDirectory = baseSettings.ZippedToolsDirectory,

        // The ports should not overlap either.
        TorSettings = { SocksPort = startingPort, ControlPort = startingPort + 1 },
        PrivoxySettings = { Port = startingPort + 2 },
    };

    using (var proxy = new TorSharpProxy(settings))
    {
        await proxy.ConfigureAndStartAsync();
        var handler = new HttpClientHandler
        {
            Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxySettings.Port))
        };

        using (handler)
        using (var httpClient = new HttpClient(handler))
        {
            var ipA = (await httpClient.GetStringAsync("https://api.ipify.org")).Trim();
            Console.WriteLine($"[Instance {name}] {ipA}");
            barrier.SignalAndWait();
            var ipB = (await httpClient.GetStringAsync("https://api.ipify.org")).Trim();
            Console.WriteLine($"[Instance {name}] {ipB}");
        }

        proxy.Stop();
    }
}

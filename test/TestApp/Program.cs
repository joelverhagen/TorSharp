using System.Net;
using Knapcode.TorSharp;
using Knapcode.TorSharp.Tests.TestSupport;

if (args.Length != 4
    || !bool.TryParse(args[0], out var writeToConsole)
    || !Enum.TryParse<ToolRunnerType>(args[1], out var toolRunnerType))
{
    Console.WriteLine("There must be exactly four command line arguments:");
    Console.WriteLine();
    Console.WriteLine("  1. a string parseable as a boolean, which is whether to write tool output to the console");
    Console.WriteLine("  2. a string parseable as a ToolRunnerType");
    Console.WriteLine("  3. the zipped tools directory");
    Console.WriteLine("  4. the extracted tools directory");
    Console.WriteLine();
    Console.WriteLine($"{args.Length} arguments were provided:");
    Console.WriteLine();
    for (var i = 0; i < args.Length; i++)
    {
        Console.WriteLine($"  {i + 1}. '{args[i]}'");
    }
    return 1;
}

using var reservedPorts = ReservedPorts.Reserve(3);

var settings = new TorSharpSettings
{
    PrivoxySettings =
    {
        Port = reservedPorts.Ports[0],
    },
    TorSettings =
    {
        SocksPort = reservedPorts.Ports[1],
        ControlPort = reservedPorts.Ports[2],
    },
    WriteToConsole = writeToConsole,
    ToolRunnerType = toolRunnerType,
    ZippedToolsDirectory = args[2],
    ExtractedToolsDirectory = args[3],
};

using (var httpClient = new HttpClient())
{
    var fetcher = new TorSharpToolFetcher(settings, httpClient);
    await fetcher.FetchAsync();
}

using (var proxy = new TorSharpProxy(settings))
{
    var handler = new HttpClientHandler
    {
        Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxySettings.Port))
    };

    using (handler)
    using (var httpClient = new HttpClient(handler))
    {
        await proxy.ConfigureAndStartAsync();
        await httpClient.GetStringAsync("http://api.ipify.org");
    }

    proxy.Stop();
}

Console.WriteLine("TestApp says it's done!");

return 0;
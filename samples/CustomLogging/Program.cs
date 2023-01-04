using System.Collections.Concurrent;
using System.Net;
using Knapcode.TorSharp;
using Knapcode.TorSharp.Tools;

var settings = new TorSharpSettings
{
    // Disable writing the tool output to this process stdout/stderr.
    WriteToConsole = false,
};

// download the tools
using (var httpClient = new HttpClient())
{
    var fetcher = new TorSharpToolFetcher(settings, httpClient);
    Console.WriteLine("Fetching tools...");
    await fetcher.FetchAsync();
}

ConcurrentQueue<string> torOutput = new ConcurrentQueue<string>();
ConcurrentQueue<string> privoxyOutput = new ConcurrentQueue<string>();

// execute
using (var proxy = new TorSharpProxy(settings))
{
    EventHandler<DataEventArgs> loggingHandler = (sender, args) =>
    {
        var executable = Path.GetFileName(args.ExecutablePath);
        if (executable.StartsWith("privoxy", StringComparison.OrdinalIgnoreCase))
        {
            privoxyOutput.Enqueue(args.Data);
        }
        else
        {
            torOutput.Enqueue(args.Data);
        }
    };

    proxy.OutputDataReceived += loggingHandler;
    proxy.ErrorDataReceived += loggingHandler;

    Console.WriteLine("Starting...");
    await proxy.ConfigureAndStartAsync();

    var handler = new HttpClientHandler
    {
        Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxySettings.Port)),
    };

    using (handler)
    using (var httpClient = new HttpClient(handler))
    {
        Console.WriteLine("Making HTTP request...");
        var response = await httpClient.GetStringAsync("https://api.ipify.org");
        Console.WriteLine("Response: " + response);
    }

    Console.WriteLine("Stopping...");
    proxy.Stop();

    Console.WriteLine();
    Console.WriteLine("==== Tor output ==== ");
    Console.WriteLine(string.Join(Environment.NewLine, torOutput.Select(x => "  " + x)).TrimEnd());
    Console.WriteLine();

    Console.WriteLine("==== Privoxy output ==== ");
    Console.WriteLine(string.Join(Environment.NewLine, privoxyOutput.Select(x => "  " + x)).TrimEnd());
    Console.WriteLine();
}

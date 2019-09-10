# TorSharp

Use Tor for your C# HTTP clients. Tor + Privoxy = :heart:

All you need is client code that can use a simple HTTP proxy.

[![Build status](https://img.shields.io/appveyor/ci/joelverhagen/torsharp.svg)](https://ci.appveyor.com/project/joelverhagen/torsharp) [![NuGet version](https://img.shields.io/nuget/v/Knapcode.TorSharp.svg)](https://www.nuget.org/packages/Knapcode.TorSharp) ![NuGet downloads](https://img.shields.io/nuget/dt/Knapcode.TorSharp.svg)

## Notice

This product is produced independently from the Tor® anonymity software and carries no guarantee from
[The Tor Project](https://www.torproject.org/) about quality, suitability or anything else.

## Details

- Supports:
  - **.NET Core** (.NET Standard 2.0 and later)
  - **.NET Framework** (.NET Framework 4.5 and later)
  - **Windows** (Windows 10 is tested, some older Windows should work too)
  - Linux support is not yet complete but in progress.
  - Mac OS X support is not planned.
- Uses Privoxy to redirect HTTP proxy traffic to Tor.
- Uses virtual desktops to manage Tor and Privoxy processes.
- Optionally downloads the latest version of Tor and Privoxy.

## Install

```
Install-Package Knapcode.TorSharp
```

## Example

```csharp
// configure
var settings = new TorSharpSettings
{
    ZippedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorZipped"),
    ExtractedToolsDirectory = Path.Combine(Path.GetTempPath(), "TorExtracted"),
    PrivoxyPort = 1337,
    TorSocksPort = 1338,
    TorControlPort = 1339,
    TorControlPassword = "foobar"
};

// download tools
await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();

// execute
var proxy = new TorSharpProxy(settings);
var handler = new HttpClientHandler
{
    Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxyPort))
};
var httpClient = new HttpClient(handler);
await proxy.ConfigureAndStartAsync();
Console.WriteLine(await httpClient.GetStringAsync("http://api.ipify.org"));
await proxy.GetNewIdentityAsync();
Console.WriteLine(await httpClient.GetStringAsync("http://api.ipify.org"));
proxy.Stop();
```

## FAQ

### The tool fetcher is throwing an exception. What do I do?

This most likely is happening because the URLs where we fetch Tor or Privoxy from are down or have changed. I would
recommend:

1. [Open an issue](https://github.com/joelverhagen/TorSharp/issues/new) so I can look into it.

1. Work around the issue by setting up the tools manually and not using `TorSharpToolFetcher`.
   [See below](#how-do-i-set-up-the-tools-manually).

1. Investigate the issue yourself. The
   [TorSharp.Sandbox](https://github.com/joelverhagen/TorSharp/blob/release/TorSharp.Sandbox/Program.cs) project is
   helpful for this. Pull requests accepted 🏆.

### How do I set up the tools manually?

If you don't want to use the `TorSharpToolFetcher` to download the latest version of the tools for you or if you want
to use a specific version of Tor and Privoxy, follow these steps.

1. Make a directory that will hold the zipped Tor and Privoxy binaries.
1. Put a Tor Win32 ZIP in that folder with the file name like: `tor-win32-{version}.zip`
   - `{version}` must be parsable as a `System.Version` meaning it is `major.minor[.build[.revision]]`.
   - Example: `tor-win32-0.3.5.8.zip`
   - The ZIP is expected to have `Tor\tor.exe`.
1. Put a Privoxy Win32 ZIP in that folder with a file name like: `privoxy-win32-{version}.zip`
   - Again, `{version}` must be parsable as a `System.Version`.
   - Example: `privoxy-win32-3.0.26.zip`
   - The ZIP is expected to have `privoxy.exe`.
1. Initialize a `TorSharpSettings` instance where `ZippedToolsDirectory` is the directory created above.
1. Pass this settings instance to the `TorSharpProxy` constructor.

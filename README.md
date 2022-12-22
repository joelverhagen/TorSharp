# TorSharp

Use Tor for your C# HTTP clients. Tor + Privoxy = :heart:

All you need is client code that can use a simple HTTP proxy.

[![NuGet downloads](https://img.shields.io/nuget/dt/Knapcode.TorSharp.svg)](https://www.nuget.org/packages/Knapcode.TorSharp) ![Build](https://github.com/joelverhagen/TorSharp/workflows/Build/badge.svg)

## Notice

This product is produced independently from the Tor® anonymity software and carries no guarantee from [The Tor Project](https://www.torproject.org/) about quality, suitability or anything else.

## Details

- Supports:
  - **.NET Core** (.NET Standard 2.0 and later)
  - **.NET Framework** (.NET Framework 4.6.2 and later)
  - **Windows**
    - ✔️ Windows 10 version 1903
    - ✔️ Windows 11 version 21H2
    - Older Windows should work too
  - **Linux**
    - ✔️ Ubuntu 20.04
    - ✔️ Ubuntu 18.04
    - ✔️ Ubuntu 16.04
    - ✔️ Debian 10
    - ⚠️ Debian 9 ([confirmed by a user](https://github.com/joelverhagen/TorSharp/issues/42#issuecomment-539403030) but [may have issues](https://github.com/joelverhagen/TorSharp/issues/64#issuecomment-774825257))
    - ⚠️ CentOS 7 supported via `ExecutablePathOverride` ([see below](#centos-7))
  - ❌ Mac OS X support is not planned. I don't have a Mac 😕
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
   PrivoxySettings = { Port = 1337 },
   TorSettings =
   {
      SocksPort = 1338,
      ControlPort = 1339,
      ControlPassword = "foobar",
   },
};

// download tools
await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();

// execute
var proxy = new TorSharpProxy(settings);
var handler = new HttpClientHandler
{
    Proxy = new WebProxy(new Uri("http://localhost:" + settings.PrivoxySettings.Port))
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

This most likely is happening because the URLs where we fetch Tor or Privoxy from are down or have changed. I would recommend:

1. [Open an issue](https://github.com/joelverhagen/TorSharp/issues/new) so I can look into it.

1. Work around the issue by setting up the tools manually and not using `TorSharpToolFetcher`. [See below](#how-do-i-set-up-the-tools-manually).

1. Investigate the issue yourself. The [TorSharp.Sandbox](https://github.com/joelverhagen/TorSharp/blob/release/samples/TorSharp.Sandbox/Program.cs) project is helpful for this. Pull requests accepted 🏆.

### How do I set up the tools manually?

If you don't want to use the `TorSharpToolFetcher` to download the latest version of the tools for you or if you want to use a specific version of Tor and Privoxy, follow these steps.

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

### Privoxy fetched by TorSharp fails to start? Try installing missing dependencies.

It's possible some expected shared libraries aren't there. Try to look at the error message and judge which library needs to be installed from your distro's package repository.

#### Ubuntu 20.04

**Problem:** On Ubuntu 20.04 the following errors may appear:

`
/tmp/TorExtracted/privoxy-linux64-3.0.29/usr/sbin/privoxy: error while loading shared libraries: libmbedtls.so.12: cannot open shared object file: No such file or directory
`

**Solution:** install a missing dependency.

```console
[joel@ubuntu]$ sudo apt install -y libmbedtls-dev
```

#### Debian 10

This includes Microsoft's .NET Core 3.1 runtime image `mcr.microsoft.com/dotnet/core/runtime:3.1` and Microsoft's .NET 5.0 runtime image `mcr.microsoft.com/dotnet/runtime:5.0`.

**Problem:** On Debian 10 the following errors may appear:

`
 /tmp/TorExtracted/privoxy-linux64-3.0.29/usr/sbin/privoxy: error while loading shared libraries: libbrotlidec.so.1: cannot open shared object file: No such file or directory
`

`
 /tmp/TorExtracted/privoxy-linux64-3.0.29/usr/sbin/privoxy: error while loading shared libraries: libmbedtls.so.12: cannot open shared object file: No such file or directory
`

**Solution:** install two missing dependencies. Thanks for [the heads up](https://github.com/joelverhagen/TorSharp/issues/64#issuecomment-774701302), [@cod3rshotout](https://github.com/cod3rshotout)!

```console
[joel@debian10]$ sudo apt-get install -y libbrotli1 libmbedtls-dev
```

### Privoxy fetched by TorSharp fails to start? Try `ExecutablePathOverride`.

On Linux, the Privoxy binaries fetched seem to be built for the latest Debian and Ubuntu distributions. I can confirm that some other distributions don't work.

I'm no Linux expert but my guess is that there are missing shared libraries that are different on the running platform than the Debian platform that Privoxy was compiled for. The easiest workaround is to install Privoxy to your system and set the `TorSharpSettings.PrivoxySetting.ExecutablePathOverride` configuration setting to `"privoxy"` (i.e. use Privoxy from PATH).

After you install it, make sure `privoxy` is in the PATH.

```console
[joel@linux]$ which privoxy
/usr/sbin/privoxy
```

After this is done, just configure TorSharp to use the system Privoxy with the `ExecutablePathOverride` setting:

```csharp
var settings = new TorSharpSettings();
settings.PrivoxySettings.ExecutablePathOverride = "privoxy";
```

Note that you may encounter warning or error messages in the output due to new configuration being used with an older executable. I haven't ran into any problems with this myself but it's possible things could get weird.

#### CentOS 7

**Problem:** the following error appears:

`
/tmp/TorExtracted/privoxy-linux64-3.0.28/usr/sbin/privoxy: error while loading shared libraries: libpcre.so.3: cannot open shared object file: No such file or directory
`

**Solution:** install Privoxy. It is available on `epel-release`.

```console
[joel@centos]$ sudo yum install epel-release -y
...
[joel@centos]$ sudo yum install privoxy -y
```

#### Debian 9

This includes Microsoft's .NET Core 2.1 runtime image: `mcr.microsoft.com/dotnet/core/runtime:2.1`.

**Problem:** the following errors may appear:

`
/tmp/TorExtracted/privoxy-linux64-3.0.29/usr/sbin/privoxy: error while loading shared libraries: libbrotlidec.so.1: cannot open shared object file: No such file or directory
`

`
/tmp/TorExtracted/privoxy-linux64-3.0.29/usr/sbin/privoxy: error while loading shared libraries: libmbedtls.so.12: cannot open shared object file: No such file or directory
`

**Solution:** install Privoxy. It is available in the default source lists.

```console
[joel@debian9]$ sudo apt-get install privoxy -y
```

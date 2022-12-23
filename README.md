# TorSharp

Use Tor for your C# HTTP clients. Tor + Privoxy = :heart:

All you need is client code that can use a simple HTTP proxy.

[![NuGet downloads](https://img.shields.io/nuget/dt/Knapcode.TorSharp.svg)](https://www.nuget.org/packages/Knapcode.TorSharp) ![Build](https://github.com/joelverhagen/TorSharp/workflows/Build/badge.svg)

## Notice

This product is produced independently from the Tor® anonymity software and carries no guarantee from [The Tor Project](https://www.torproject.org/) about quality, suitability or anything else.

## Details

- Supports:
  - **.NET Core** (.NET Standard 2.0 and later, including .NET 5+)
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
dotnet add package Knapcode.TorSharp
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

### Can I run multiple instances in parallel?

Yes, you can. See this sample: [`samples/MultipleInstances/Program.cs`](https://github.com/joelverhagen/TorSharp/tree/release/samples/MultipleInstances/Program.cs).

However, you need to adhere to the following guidance.

None of the types in this library should be considered thread safe. Use separate instances for each parallel task/thread.
- `TorSharpProxy`: this is stateful and should not be shared.
- `TorSharpSettings`: the values held in the settings class need to be different for parallel threads, so it doesn't make sense to share instances.
- `TorSharpToolFetcher`: this is stateless so it may be safe, but I would keep this a singleton since you shouldn't have multiple copies of the zipped tools (just multiple copies of the *extracted tools*).

Parallel threads must have different values for these settings. The defaults will not work.

- **Must be made unique by you:**
  - `TorSharpSettings.ExtractedToolsDirectory`: this is the parent directory of the tool working directories. Specify a different value for each thread. In the sample above, I see each parallel task to be a sibling directory, e.g. `{some_root}/a`, `{some_root}/b`, etc.
  - `TorSharpSettings.PrivoxySettings.Port`: this is the Privoxy listen port. Each Privoxy process needs its own port. Can be ignored if `TorSharpSettings.PrivoxySettings.Disable` is `true`.
  - `TorSharpSettings.TorSettings.SocksPort`: this is the Tor SOCKS listen port. Each Tor process needs its own port.
- **Must be unique, but only if you set them:**
  - `TorSharpSettings.VirtualDesktopName`: this is automatically generated based on `ExtractedToolsDirectory`, but if you manually set it, it must be unique.
  - `TorSharpSettings.TorSettings.ControlPort`: this is the Tor SOCKS listen port. Each Tor process needs its own port.
  - `TorSharpSettings.TorSettings.AdditionalSockPorts`: if used, it must have unique values.
  - `TorSharpSettings.TorSettings.HttpTunnelPort`: if used, it must have a unique value.
  - `TorSharpSettings.TorSettings.DataDirectory`: the default is based `ExtractedToolsDirectory`, but if you manually set it, it must be unique.

In general, directory configuration values must be different from all of the other directories, except `TorSharpSettings.ZippedToolsDirectory` which should not be downloaded to in parallel by `TorSharpToolFetcher` but can be read from in parallel with multiple `TorSharpProxy` instances. Port configuration values need to all be unique.

### Privoxy fetched by TorSharp fails to start? Try installing missing dependencies.

It's possible some expected shared libraries aren't there. Try to look at the error message and judge which library needs to be installed from your distro's package repository.

#### Ubuntu 20.04

**Problem:** On Ubuntu 20.04 the following errors may appear:

`
/tmp/TorExtracted/privoxy-linux64-3.0.29/usr/sbin/privoxy: error while loading shared libraries: libmbedtls.so.12: cannot open shared object file: No such file or directory
`

**Solution:** install the missing dependencies. Note that these steps **install packages from the Debian repository**. This is very much not recommended by official guidance. For my own testing, it works well enough. I use this trick in the GitHub Action workflow. ⚠️ Do this at your own risk!

```console
[joel@ubuntu]$ curl -O http://ftp.us.debian.org/debian/pool/main/m/mbedtls/libmbedcrypto3_2.16.0-1_amd64.deb
[joel@ubuntu]$ curl -O http://ftp.us.debian.org/debian/pool/main/m/mbedtls/libmbedx509-0_2.16.0-1_amd64.deb
[joel@ubuntu]$ curl -O http://ftp.us.debian.org/debian/pool/main/m/mbedtls/libmbedtls12_2.16.0-1_amd64.deb
[joel@ubuntu]$ echo "eb6751c98adfdf0e7a5a52fbd1b5a284ffd429e73abb4f0a4497374dd9f142c7 libmbedcrypto3_2.16.0-1_amd64.deb" > SHA256SUMS
[joel@ubuntu]$ echo "e8ea5dd71b27591c0f55c1d49f597d3d3b5c747bde25d3b3c3b03ca281fc3045 libmbedx509-0_2.16.0-1_amd64.deb" >> SHA256SUMS
[joel@ubuntu]$ echo "786c7e7805a51f3eccd449dd44936f735afa97fc5f3702411090d8b40f0e9eda libmbedtls12_2.16.0-1_amd64.deb" >> SHA256SUMS
[joel@ubuntu]$ sha256sum -c SHA256SUMS || { echo "Checksum failed. Aborting."; exit 1; }
[joel@ubuntu]$ sudo apt-get install -y --allow-downgrades ./*.deb
```

I have the checksum verification in there because these pages have SSL problems and the advertised URL is HTTP not HTTPS (by design I think).

#### Debian 10

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

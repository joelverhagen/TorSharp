# initialize
$root = $PSScriptRoot
$build = Join-Path $root "build"
$dotnetDir = Join-Path $build "dotnet"
$dotnetInstallUrl = "https://raw.githubusercontent.com/dotnet/cli/b4ef34bc38370d85d9089582a9fba9cce00dae28/scripts/obtain/dotnet-install.ps1"
$dotnetInstall = Join-Path $dotnetDir "dotnet-install.ps1"
$sln = Join-Path $root "Knapcode.TorSharp.sln"

$dotnet = Join-Path $dotnetDir "dotnet.exe"

# get tools
if (-Not (Test-Path $dotnetDir)) {
    New-Item $dotnetDir -Type Directory
}

if (-Not (Test-Path $dotnetInstall)) {
    Invoke-WebRequest -Uri $dotnetInstallUrl -OutFile $dotnetInstall
}

& $dotnetInstall -i $dotnetDir -Version 1.0.0-preview4-004233

# restore
& $dotnet restore $sln

# build
& $dotnet build $sln

# test
& $dotnet test (Join-Path $root "test/Knapcode.TorSharp.Tests/Knapcode.TorSharp.Tests.csproj")

# pack
& $dotnet pack (Join-Path $root "src/Knapcode.TorSharp/Knapcode.TorSharp.csproj")

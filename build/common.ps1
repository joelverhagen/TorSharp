$repoDir = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$buildDir = Join-Path $repoDir "build"
$packagesDir = Join-Path $repoDir "build\packages"
$nuget = Join-Path $buildDir "nuget.exe"
$xunit32 = Join-Path $packagesDir "xunit.runner.console\tools\net472\xunit.console.x86.exe"
$xunit64 = Join-Path $packagesDir "xunit.runner.console\tools\net472\xunit.console.exe"

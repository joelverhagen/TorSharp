$repoDir = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$buildDir = Join-Path $repoDir "build"
$packagesDir = Join-Path $repoDir "build\packages"
$nuget = Join-Path $buildDir "nuget.exe"
$xunit32 = Join-Path $packagesDir "xunit.runner.console\tools\net472\xunit.console.x86.exe"
$xunit64 = Join-Path $packagesDir "xunit.runner.console\tools\net472\xunit.console.exe"

# https://stackoverflow.com/a/48999101
function Exec {
    [CmdletBinding()]
    param(
        [Parameter(Position=0, Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1, Mandatory=0)][string]$errorMessage = ("Error executing command: {0}" -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

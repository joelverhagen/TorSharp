$artifactsDir = Join-Path $PSScriptRoot "artifacts"
$buildDir = Join-Path $PSScriptRoot "build"
$nuget = Join-Path $buildDir "nuget.exe"

if (!(Test-Path $buildDir)) {
    New-Item -Type Directory $buildDir | Out-Null
}

if (!(Test-Path $nuget)) {
    Write-Host "[ Downloading nuget.exe... ]" -ForegroundColor DarkGreen
    $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/v5.1.0/nuget.exe" -OutFile $nuget
    $ProgressPreference = 'Continue'

    Write-Host "Downloaded nuget.exe to $nuget"
}

Write-Host "[ Building the package... ]" -ForegroundColor DarkGreen
$csproj = Join-Path $PSScriptRoot "TorSharp\TorSharp.csproj"
& $nuget pack $csproj -OutputDirectory $artifactsDir -Properties Configuration=Release -Verbosity detailed
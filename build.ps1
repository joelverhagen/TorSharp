. (Join-Path $PSScriptRoot "build\common.ps1")

if (!(Test-Path $buildDir)) {
    New-Item -Type Directory $buildDir | Out-Null
}

if (!(Test-Path $nuget)) {
    Write-Host "Downloading nuget.exe..." -ForegroundColor DarkGreen
    $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/v6.4.0/nuget.exe" -OutFile $nuget
    $ProgressPreference = 'Continue'

    Write-Host "Downloaded nuget.exe to $nuget"
}

Write-Host "[ Restoring solution packages ]" -ForegroundColor DarkGreen
Exec { & $nuget install (Join-Path $repoDir "packages.config") -Output $packagesDir -ExcludeVersion }
if ($LASTEXITCODE -ne 0) {
    throw "Failed to restore solution packages."
}

& (Join-Path $repoDir "build\set-version.ps1")

Write-Host "[ Building ]" -ForegroundColor DarkGreen
Exec { dotnet build --configuration Release /p:ContinuousIntegrationBuild=true }

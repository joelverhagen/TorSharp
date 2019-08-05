Write-Host "[ Determining release version ]" -ForegroundColor DarkGreen
$version = (Get-Content (Join-Path $PSScriptRoot "version.txt")).Trim()

[int]$buildNumber = $null
if ([int]::TryParse($env:APPVEYOR_BUILD_NUMBER, [ref]$buildNumber) ) {
    $version = $version + "-ci-" + $buildNumber
}

$env:TorSharpVersion = $version
Write-Host "Using: $version"

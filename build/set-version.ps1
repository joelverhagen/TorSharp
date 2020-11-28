Write-Host "[ Determining release version ]" -ForegroundColor DarkGreen
$version = (Get-Content (Join-Path $PSScriptRoot "version.txt")).Trim()

if ($env:GITHUB_REF) {
    $branchPart = "." + $env:GITHUB_REF.Split('/')[-1]
}

[int]$buildNumber = $null
if ([int]::TryParse($env:GITHUB_RUN_NUMBER, [ref]$buildNumber) ) {
    $version = $version + "-ci$branchPart.$buildNumber"
}

$env:TorSharpVersion = $version
Write-Host "Using: $version"

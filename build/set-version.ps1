Write-Host "[ Determining release version ]" -ForegroundColor DarkGreen
$version = (Get-Content (Join-Path $PSScriptRoot "version.txt")).Trim()

if ($env:GITHUB_HEAD_REF) {
    $ref = $env:GITHUB_HEAD_REF
} elseif ($env:GITHUB_REF) {
    $ref = $env:GITHUB_REF
}

if ($ref) {
    $dotBranch = "." + $ref.Split('/')[-1]
}

[int]$buildNumber = $null
if ([int]::TryParse($env:GITHUB_RUN_NUMBER, [ref]$buildNumber) ) {
    $version = $version + "-ci$dotBranch.$buildNumber"
}

$env:TorSharpVersion = $version
Write-Host "Using: $version"

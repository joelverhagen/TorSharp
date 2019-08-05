. (Join-Path $PSScriptRoot "build\common.ps1")

$options = ""
if ($env:APPVEYOR) {
    $options = "-appveyor"
}

$testAssembly = "artifacts\TorSharp\TorSharp.Tests\bin\Release\net452\Knapcode.TorSharp.Tests.dll"

if (Test-Path $testAssembly) {
    Write-Host "[ Running tests in 32-bit ]" -ForegroundColor DarkGreen
    & $xunit32 $testAssembly $options

    Write-Host "[ Running tests in 64-bit ]" -ForegroundColor DarkGreen
    & $xunit64 $testAssembly $options
} else {
    Write-Host "[ Running tests with .NET CLI ]" -ForegroundColor DarkGreen
    dotnet test
}

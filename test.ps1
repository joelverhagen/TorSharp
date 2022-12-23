. (Join-Path $PSScriptRoot "build\common.ps1")

$netFrameworkTestAssembly = "artifacts\TorSharp\TorSharp.Tests\bin\Release\net472\Knapcode.TorSharp.Tests.dll"

if (Test-Path $netFrameworkTestAssembly) {
    Write-Host "[ Running .NET Framework tests in 32-bit ]" -ForegroundColor DarkGreen
    Exec { & $xunit32 $netFrameworkTestAssembly }

    Write-Host "[ Running .NET Framework tests in 64-bit ]" -ForegroundColor DarkGreen
    Exec { & $xunit64 $netFrameworkTestAssembly }
}
else {
    Write-Host "[ Building .NET Framework tests with .NET CLI ]" -ForegroundColor DarkGreen
    Exec { dotnet build test\TorSharp.Tests --framework net472 }

    Write-Host "[ Running .NET Framework tests with .NET CLI ]" -ForegroundColor DarkGreen
    Exec { dotnet test test\TorSharp.Tests --no-build --framework net472 --verbosity normal }
}

Write-Host "[ Building .NET Core tests with .NET CLI ]" -ForegroundColor DarkGreen
Exec { dotnet build test\TorSharp.Tests --framework net6.0 }

Write-Host "[ Running .NET Core tests with .NET CLI ]" -ForegroundColor DarkGreen
Exec { dotnet test test\TorSharp.Tests --no-build --framework net6.0 --verbosity normal }

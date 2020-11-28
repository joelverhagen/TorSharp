. (Join-Path $PSScriptRoot "build\common.ps1")

$options = ""
if ($env:APPVEYOR) {
    $options = "-appveyor"
}

$netFrameworkTestAssembly = "artifacts\TorSharp\TorSharp.Tests\bin\Release\net452\Knapcode.TorSharp.Tests.dll"

if (Test-Path $netFrameworkTestAssembly) {
    Write-Host "[ Running .NET Framework tests in 32-bit ]" -ForegroundColor DarkGreen
    Exec { & $xunit32 $netFrameworkTestAssembly $options }

    Write-Host "[ Running .NET Framework tests in 64-bit ]" -ForegroundColor DarkGreen
    Exec { & $xunit64 $netFrameworkTestAssembly $options }
} else {
	Write-Host "[ Building .NET Framework tests with .NET CLI ]" -ForegroundColor DarkGreen
	Exec { dotnet build test\TorSharp.Tests --framework net452 }

	Write-Host "[ Running .NET Framework tests with .NET CLI ]" -ForegroundColor DarkGreen
	Exec { dotnet test test\TorSharp.Tests --no-build --framework net452 --verbosity normal }
}

Write-Host "[ Building .NET Core tests with .NET CLI ]" -ForegroundColor DarkGreen
Exec { dotnet build test\TorSharp.Tests --framework netcoreapp2.1 }

Write-Host "[ Running .NET Core tests with .NET CLI ]" -ForegroundColor DarkGreen
Exec { dotnet test test\TorSharp.Tests --no-build --framework netcoreapp2.1 --verbosity normal }

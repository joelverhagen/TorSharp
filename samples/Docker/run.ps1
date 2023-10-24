Push-Location
try {
    $root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ../..))
    Set-Location $root
    docker build . -t docker-repro --no-cache --progress plain -f (Join-Path $PSScriptRoot "Dockerfile")    
}
catch {
    Pop-Location
}


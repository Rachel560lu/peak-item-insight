$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.dotnet-home'
$dotnetPath = Join-Path $projectRoot '.dotnet-sdk\dotnet.exe'
& (Join-Path $PSScriptRoot 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Production build failed.' }
& $dotnetPath run --project (Join-Path $projectRoot 'tools\Verification\Verification.csproj') --configuration Release
if ($LASTEXITCODE -ne 0) { throw 'Offline verification failed.' }

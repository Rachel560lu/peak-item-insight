$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.dotnet-home'
$dotnetPath = Join-Path $projectRoot '.dotnet-sdk\dotnet.exe'
& (Join-Path $PSScriptRoot 'build.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Production build failed.' }
& $dotnetPath build (Join-Path $projectRoot 'tools\Verification\Verification.csproj') --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Verification build failed.' }
# Use the bundled .NET host; this machine blocks generated apphost executables.
& $dotnetPath (Join-Path $projectRoot 'tools\Verification\bin\Release\net8.0\Verification.dll')
if ($LASTEXITCODE -ne 0) { throw 'Offline verification failed.' }

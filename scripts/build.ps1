param(
    [string]$Configuration = "Release",
    [string]$PeakManagedDir = "D:\SteamLibrary\steamapps\common\PEAK\PEAK_Data\Managed"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$dotnet = Join-Path $projectRoot ".dotnet-sdk\dotnet.exe"

if (-not (Test-Path $dotnet)) {
    $dotnet = "dotnet"
}

$env:DOTNET_CLI_HOME = Join-Path $projectRoot ".dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

& $dotnet build (Join-Path $projectRoot "PeakItemInsight.csproj") `
    -c $Configuration `
    --configfile (Join-Path $projectRoot "NuGet.Config") `
    -p:PeakManagedDir=$PeakManagedDir

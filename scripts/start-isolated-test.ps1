param([switch]$SmokeTest, [switch]$WorldTest, [switch]$ExitAfterSmoke)
$ErrorActionPreference = 'Stop'
if ($ExitAfterSmoke -and !$SmokeTest) { throw 'ExitAfterSmoke requires SmokeTest.' }
$gameDir = 'D:\SteamLibrary\steamapps\common\PEAK'
$profileDir = 'C:\Users\midor\AppData\Roaming\r2modmanPlus-local\PEAK\profiles\PeakItemInsight-Test'
$taskRoot = Split-Path -Parent $PSScriptRoot
$testDir = Join-Path $taskRoot 'backups\detailed-mode-engine'
$loaderDir = Join-Path $testDir 'BepInEx'
& (Join-Path $PSScriptRoot 'assert-peak-stopped.ps1')
$coreDir = Join-Path $loaderDir 'core'
if (!(Test-Path -LiteralPath $coreDir)) {
    New-Item -ItemType Directory -Force $loaderDir | Out-Null
    Copy-Item -LiteralPath (Join-Path $profileDir 'BepInEx\core') -Destination $coreDir -Recurse
}
$plugins = Join-Path $loaderDir 'plugins'
New-Item -ItemType Directory -Force $plugins | Out-Null
$pluginDlls = @(Get-ChildItem -LiteralPath $plugins -Recurse -Filter '*.dll')
if (@($pluginDlls | Where-Object Name -ne 'PeakItemInsight.dll').Count -gt 0) { throw 'Unexpected plugins in workspace isolation.' }
Copy-Item -LiteralPath (Join-Path $taskRoot 'bin\Release\netstandard2.1\PeakItemInsight.dll') -Destination (Join-Path $plugins 'PeakItemInsight.dll')
$loader = Join-Path $coreDir 'BepInEx.Preloader.dll'
$log = Join-Path $testDir ('player-isolated-' + (Get-Date -Format 'yyyyMMddTHHmmss') + '.log')
$arguments = @('--doorstop-enabled', 'true', '--doorstop-target-assembly', ('"' + $loader + '"'), '-logFile', ('"' + $log + '"'))
if ($SmokeTest) { $arguments += '-insightSmokeTest' }
if ($ExitAfterSmoke) { $arguments += '-insightSmokeTestExit' }
if ($WorldTest) { $arguments += '-insightWorldTest' }
$windowStyle = if ($SmokeTest) { 'Hidden' } else { 'Normal' }
# Native executable/assets stay in Steam's installation. Only the loader, plugin,
# configs and diagnostics use our workspace. No game-directory junction/appid edits.
# Steam may relaunch this process with the same arguments; validate the final PID's trace.
$testProcess = Start-Process -FilePath (Join-Path $gameDir 'PEAK.exe') -WorkingDirectory $gameDir -ArgumentList $arguments -WindowStyle $windowStyle -PassThru
Write-Output "TEST_PID=$($testProcess.Id) START=$($testProcess.StartTime.ToString('O')) PLAYER_LOG=$log"
if (!$SmokeTest) {
    Write-Output 'Use Create room / Host for a solo gameplay test. The previous offline session failed native player registration.'
}

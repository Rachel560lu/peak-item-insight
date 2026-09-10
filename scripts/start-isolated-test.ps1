param([switch]$SmokeTest, [switch]$WorldTest, [switch]$ExitAfterSmoke)
$ErrorActionPreference = 'Stop'
if ($ExitAfterSmoke -and !$SmokeTest) { throw 'ExitAfterSmoke requires SmokeTest.' }
$gameDir = 'D:\SteamLibrary\steamapps\common\PEAK'
$profileDir = 'C:\Users\midor\AppData\Roaming\r2modmanPlus-local\PEAK\profiles\PeakItemInsight-Test'
$appidPath = Join-Path $gameDir 'steam_appid.txt'
& (Join-Path $PSScriptRoot 'assert-peak-stopped.ps1')
$link = Get-Item -LiteralPath (Join-Path $gameDir 'BepInEx')
if ($link.LinkType -ne 'Junction' -or $link.Target -ne (Join-Path $profileDir 'BepInEx')) { throw 'Test profile junction is not the expected target.' }
if (Test-Path -LiteralPath $appidPath) { throw 'Existing steam_appid.txt found; refusing to overwrite.' }
$pluginDlls = @(Get-ChildItem -LiteralPath (Join-Path $profileDir 'BepInEx\plugins') -Recurse -Filter '*.dll')
if ($pluginDlls.Count -ne 1 -or $pluginDlls[0].Name -ne 'PeakItemInsight.dll') { throw 'Unexpected test plugins.' }
try {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'steam_appid.txt') -Destination $appidPath
    $arguments = @('--doorstop-enabled', 'true', '--doorstop-target-assembly', ('"' + (Join-Path $profileDir 'BepInEx\core\BepInEx.Preloader.dll') + '"'))
    if ($SmokeTest) { $arguments += '-insightSmokeTest' }
    if ($ExitAfterSmoke) { $arguments += '-insightSmokeTestExit' }
    if ($WorldTest) { $arguments += '-insightWorldTest' }
    $windowStyle = if ($SmokeTest) { 'Hidden' } else { 'Normal' }
    $testProcess = Start-Process -FilePath (Join-Path $gameDir 'PEAK.exe') -WorkingDirectory $gameDir -ArgumentList $arguments -WindowStyle $windowStyle -PassThru
    Write-Output "TEST_PID=$($testProcess.Id) START=$($testProcess.StartTime.ToString('O'))"
    $testProcess.WaitForExit()
    Write-Output "TEST_EXIT=$($testProcess.ExitCode)"
}
finally {
    # Remove only our exact, unchanged development file, never an existing user file.
    if ((Test-Path -LiteralPath $appidPath) -and (Get-Content -LiteralPath $appidPath -Raw).Trim() -eq '3527290') {
        Remove-Item -LiteralPath $appidPath
        Write-Output 'Temporary steam_appid.txt removed.'
    }
}

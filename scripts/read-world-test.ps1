param([string]$LogPath)
$ErrorActionPreference = 'Stop'
$traceDir='C:\Users\midor\AppData\Roaming\r2modmanPlus-local\PEAK\profiles\PeakItemInsight-Test\BepInEx\InsightDiagnostics'
if (!$LogPath) {
    $gameProcess=@(Get-CimInstance Win32_Process -Filter "Name='PEAK.exe'" | Where-Object { $_.ThreadCount -ne 0 -or $_.HandleCount -ne 0 })
    if (@($gameProcess).Count -ne 1) { throw 'Expected exactly one PEAK process.' }
    $LogPath=(Get-ChildItem -LiteralPath $traceDir -Filter "session-$($gameProcess[0].ProcessId)-*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
}
if (!$LogPath) { throw 'No trace for current game PID.' }
$lines=Get-Content -LiteralPath $LogPath
$predictions=@($lines | Where-Object { $_ -match ' WORLD_PREDICTION ' })
$hud=@($lines | Where-Object { $_ -match ' WORLD_HUD | HUNGER_PULSE | INJURY_PULSE ' })
$unbound=@($lines | Where-Object { $_ -match ' (HUNGER|INJURY)_PULSE_UNBOUND ' })
$errors=@($lines | Where-Object { $_ -match ' ERROR ' })
$hide=@($lines | Where-Object { $_ -match ' HIDE ' })
$samples=@($lines | Where-Object { $_ -match ' WORLD_OBSERVED ' })
[pscustomobject]@{
    Log=$LogPath
    Predictions=$predictions.Count
    HudSamples=$hud.Count
    HideEvents=$hide.Count
    ObservedStateSamples=$samples.Count
    Errors=$errors.Count
    RecoveryBindFailures=$unbound.Count
    Verdict=if ($errors.Count) {'FAIL: runtime errors'} elseif (!$predictions.Count) {'PENDING: no real-item preview recorded'} elseif (!$hud.Count) {'PENDING: no native HUD sample recorded'} else {'EVIDENCE AVAILABLE: compare predictions, HUD and actual effects; not an automatic end-to-end PASS'}
} | Format-List
$predictions | Select-Object -Last 5
$hud | Select-Object -Last 3
$samples | Select-Object -Last 8

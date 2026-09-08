$ErrorActionPreference = 'Stop'
# Windows can retain a terminated process object with no executable threads or
# handles. Unknown metadata is not treated as proof of exit.
$peakRecords = @(Get-CimInstance Win32_Process -Filter "Name='PEAK.exe'")
foreach ($peakRecord in $peakRecords) {
    if ($null -ne $peakRecord.ThreadCount -and $null -ne $peakRecord.HandleCount -and
        $peakRecord.ThreadCount -eq 0 -and $peakRecord.HandleCount -eq 0) {
        Write-Output "Ignoring terminated PEAK record PID=$($peakRecord.ProcessId) threads=0 handles=0"
    } else { throw "Close PEAK before continuing (PID=$($peakRecord.ProcessId))." }
}

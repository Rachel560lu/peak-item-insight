param([Parameter(Mandatory = $true)][string]$ZipPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = Split-Path -Parent $PSScriptRoot
$verify = Join-Path $PSScriptRoot 'verify-package.ps1'
$null = & $verify -ZipPath $ZipPath
$directory = Join-Path $root ('dist/validator-tests-' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $directory
# Deliberately invalid copies stay outside the release candidate directory.
$cases = @(
    @{ Name = 'missing-readme'; Target = 'README.md'; Replacement = $null; Expected = 'entry count' },
    @{ Name = 'tampered-dll'; Target = 'BepInEx/plugins/PeakItemInsight/PeakItemInsight.dll'; Replacement = 'not the verified DLL'; Expected = 'DLL does not match' },
    @{ Name = 'extra-file'; Target = 'private-log.txt'; Replacement = 'must not ship'; Expected = 'entry count' },
    @{ Name = 'invalid-icon'; Target = 'icon.png'; Replacement = 'not a PNG image'; Expected = 'not PNG' }
)
foreach ($case in $cases) {
    $copy = Join-Path $directory ($case.Name + '.zip')
    Copy-Item -LiteralPath $ZipPath -Destination $copy
    $archive = [System.IO.Compression.ZipFile]::Open($copy, [System.IO.Compression.ZipArchiveMode]::Update)
    try {
        $entry = $archive.GetEntry($case.Target)
        if ($entry) { $entry.Delete() }
        if ($null -ne $case.Replacement) {
            $entry = $archive.CreateEntry($case.Target)
            $writer = [System.IO.StreamWriter]::new($entry.Open())
            try { $writer.Write($case.Replacement) } finally { $writer.Dispose() }
        }
    } finally { $archive.Dispose() }
    $rejection = $null
    try { $null = & $verify -ZipPath $copy } catch { $rejection = $_.Exception.Message }
    if (-not $rejection -or $rejection -notmatch $case.Expected) { throw "Validator test failed: $($case.Name); result: $rejection" }
    Write-Output "PASS rejected $($case.Name): $rejection"
}
Write-Output 'RESULT valid candidate accepted; 4 invalid packages rejected. No game or installed profile touched.'

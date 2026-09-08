$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = Split-Path -Parent $PSScriptRoot
$material = Join-Path $root 'thunderstore'
$manifest = Get-Content -LiteralPath (Join-Path $material 'manifest.json') -Raw | ConvertFrom-Json
$evidence = Get-Content -LiteralPath (Join-Path $material 'release-evidence.json') -Raw | ConvertFrom-Json
$source = Get-Content -LiteralPath (Join-Path $root 'src/Plugin.cs') -Raw
$version = [regex]::Match($source, 'PluginVersion\s*=\s*"([^"]+)"').Groups[1].Value
if ($version -ne $manifest.version_number -or $version -ne $evidence.plugin_version) { throw 'Source/manifest/evidence version mismatch.' }
$dll = Join-Path $root 'dist/PeakItemInsight.dll'
if ((Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash -ne $evidence.dll_sha256) { throw 'DLL differs from evidence pin. Test and review before updating the evidence.' }
& (Join-Path $PSScriptRoot 'create-package-icon.ps1')
$folder = Join-Path $root ('dist/package-' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $folder
$zipPath = Join-Path $folder ("PeakItemInsight-$version.zip")
$archive = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    $files = @('manifest.json', 'README.md', 'CHANGELOG.md', 'icon.png')
    if (Test-Path -LiteralPath (Join-Path $material 'LICENSE')) { $files += 'LICENSE' }
    foreach ($name in $files) {
        $null = [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $material $name), $name)
    }
    $null = [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $dll, 'BepInEx/plugins/PeakItemInsight/PeakItemInsight.dll')
} finally { $archive.Dispose() }
$report = & (Join-Path $PSScriptRoot 'verify-package.ps1') -ZipPath $zipPath
# Generated report is intentionally outside the distributable archive.
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $folder 'validation.json') -Encoding UTF8
Write-Output "Validated local candidate: $zipPath"
$report
Write-Warning 'NOT uploaded. License, standard startup and real-game acceptance gates remain; see docs/release-checklist.md.'

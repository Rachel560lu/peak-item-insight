$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = Split-Path -Parent $PSScriptRoot
$material = Join-Path $root 'thunderstore'
$manifest = Get-Content -LiteralPath (Join-Path $material 'manifest.json') -Raw | ConvertFrom-Json
$evidence = Get-Content -LiteralPath (Join-Path $material 'release-evidence.json') -Raw | ConvertFrom-Json
$source = Get-Content -LiteralPath (Join-Path $root 'src/Plugin.cs') -Raw
$version = [regex]::Match($source, 'PluginVersion\s*=\s*"([^"]+)"').Groups[1].Value
$packageVersion = if ($evidence.package_version) { $evidence.package_version } else { $evidence.plugin_version }
if ($version -ne $evidence.plugin_version -or $manifest.version_number -ne $packageVersion) { throw 'Source/plugin or manifest/package evidence version mismatch.' }
if ($packageVersion -ne $version -and $evidence.change_type -ne 'documentation_only') { throw 'Different package/plugin versions require explicit documentation-only evidence.' }
$dll = Join-Path $root 'dist/PeakItemInsight.dll'
if ((Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash -ne $evidence.dll_sha256) { throw 'DLL differs from evidence pin. Test and review before updating the evidence.' }
& (Join-Path $PSScriptRoot 'create-package-icon.ps1')
$folder = Join-Path $root ('dist/package-' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $folder
$zipPath = Join-Path $folder ("PeakItemInsight-$packageVersion.zip")
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
Write-Warning 'This command does not upload. Review current release evidence and known limitations before publishing.'

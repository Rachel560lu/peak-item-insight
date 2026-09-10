param([Parameter(Mandatory = $true)][string]$ZipPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$evidence = Get-Content -LiteralPath (Join-Path $root 'thunderstore/release-evidence.json') -Raw | ConvertFrom-Json
$archive = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $ZipPath).Path)
function Read-EntryBytes($entry) {
    $stream = $entry.Open()
    $memory = [System.IO.MemoryStream]::new()
    try { $stream.CopyTo($memory); return ,$memory.ToArray() }
    finally { $stream.Dispose(); $memory.Dispose() }
}
function Read-EntryText($entry) {
    $utf8 = [System.Text.UTF8Encoding]::new($false, $true)
    return $utf8.GetString((Read-EntryBytes $entry)).TrimStart([char]0xFEFF)
}
try {
    $expected = @('manifest.json', 'README.md', 'CHANGELOG.md', 'icon.png', 'BepInEx/plugins/PeakItemInsight/PeakItemInsight.dll')
    if (Test-Path -LiteralPath (Join-Path $root 'thunderstore/LICENSE')) { $expected += 'LICENSE' }
    $names = @($archive.Entries | ForEach-Object { $_.FullName })
    if ($names.Count -ne $expected.Count) { throw 'Unexpected ZIP entry count (extra, missing or duplicate files).' }
    foreach ($name in $expected) {
        if (@($names | Where-Object { $_ -ceq $name }).Count -ne 1) { throw "Missing or duplicated case-sensitive entry: $name" }
    }
    $manifest = Read-EntryText ($archive.GetEntry('manifest.json')) | ConvertFrom-Json
    if ($manifest.name -cne 'PeakItemInsight') { throw 'Unexpected package name.' }
    if ($manifest.version_number -notmatch '^\d+\.\d+\.\d+$') { throw 'Version must be numeric major.minor.patch.' }
    $packageVersion = if ($evidence.package_version) { $evidence.package_version } else { $evidence.plugin_version }
    if ($manifest.version_number -ne $packageVersion) { throw 'Manifest differs from evidence-pinned package version.' }
    if ($packageVersion -ne $evidence.plugin_version -and $evidence.change_type -ne 'documentation_only') { throw 'Different package/plugin versions require documentation-only evidence.' }
    if (-not $manifest.description -or $manifest.description.Length -gt 250) { throw 'Invalid description length.' }
    if ($null -eq $manifest.website_url) { throw 'website_url is required (empty string is allowed).' }
    if ($manifest.website_url -and $manifest.website_url -notmatch '^https?://\S+$') { throw 'Invalid website_url.' }
    if (@($manifest.dependencies).Count -ne 1 -or $manifest.dependencies[0] -cne 'BepInEx-BepInExPack_PEAK-5.4.2403') { throw 'Dependency differs from reviewed runtime package.' }
    foreach ($name in @('README.md', 'CHANGELOG.md')) {
        $content = Read-EntryText ($archive.GetEntry($name))
        if ([string]::IsNullOrWhiteSpace($content)) { throw "Empty $name" }
        # A drive prefix must not be the final letter of https://.
        if ($content -match '(?i)(?<![a-z])[A-Z]:[\\/]|AppData[\\/]|steam_appid\.txt|session-\d+-') { throw "Potential local development data in $name" }
    }
    $bytes = Read-EntryBytes ($archive.GetEntry('icon.png'))
    if ([BitConverter]::ToString($bytes, 0, 8) -ne '89-50-4E-47-0D-0A-1A-0A') { throw 'Icon is not PNG.' }
    $memory = [System.IO.MemoryStream]::new($bytes, $false)
    try {
        $icon = [System.Drawing.Image]::FromStream($memory)
        try { if ($icon.Width -ne 256 -or $icon.Height -ne 256) { throw 'Icon must be 256x256.' } }
        finally { $icon.Dispose() }
    } finally { $memory.Dispose() }
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { $dllHash = [BitConverter]::ToString($sha.ComputeHash((Read-EntryBytes ($archive.GetEntry($expected[4]))))).Replace('-', '') }
    finally { $sha.Dispose() }
    if ($dllHash -ne $evidence.dll_sha256) { throw 'Packaged DLL does not match verified release evidence.' }
    [pscustomobject]@{
        FormatValidation = 'PASS (local checks only)'
        Version = $manifest.version_number
        PluginVersion = $evidence.plugin_version
        Files = $names
        DllSHA256 = $dllHash
        ZipSHA256 = (Get-FileHash -LiteralPath $ZipPath -Algorithm SHA256).Hash
        ReleaseStatus = 'CANDIDATE ONLY; see docs/release-checklist.md'
    }
} finally { $archive.Dispose() }

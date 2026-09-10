param(
    [string]$FfmpegPath,
    [string]$SourceDir = 'C:\Users\midor\Desktop'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (!$FfmpegPath) {
    $FfmpegPath = (Get-ChildItem (Join-Path $root '.media-tools/imageio_ffmpeg/binaries') -Filter '*.exe' | Select-Object -First 1).FullName
}
if (!(Test-Path -LiteralPath $FfmpegPath)) { throw 'Provide a local ffmpeg executable.' }
$out = Join-Path $root 'docs/assets/demos'
$null = New-Item -ItemType Directory -Force -Path $out
$clips = @(
    @{ Input = '梅子有毒.mp4'; Output = 'poisonous-food.gif'; Duration = '4.2' },
    @{ Input = '蘑菇无毒预览.mp4'; Output = 'non-poisonous-food.gif'; Duration = '2.3' },
    @{ Input = '物品简介.png'; Output = 'item-description.gif' }
)
foreach ($clip in $clips) {
    $inputPath = Join-Path $SourceDir $clip.Input
    if (!(Test-Path -LiteralPath $inputPath)) { throw "Missing demonstration: $inputPath" }
    # Keep the whole HUD, original speed and chronological order; trim only the
    # trailing pause-menu section. No invented overlays; GIF has no audio.
    $filter = 'fps=10,scale=1100:-1:flags=lanczos,split[a][b];[a]palettegen=max_colors=128:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=5:diff_mode=rectangle'
    if ($clip.Input.EndsWith('.png')) { $filter = $filter.Replace('fps=10,', '') }
    $durationArgs = @()
    if ($clip.Duration) { $durationArgs = @('-t', $clip.Duration) }
    & $FfmpegPath -hide_banner -loglevel error -y @durationArgs -i $inputPath -an -filter_complex $filter -loop 0 (Join-Path $out $clip.Output)
    if ($LASTEXITCODE -ne 0) { throw "GIF conversion failed: $inputPath" }
}
Copy-Item -LiteralPath (Join-Path $SourceDir '物品简介.png') -Destination (Join-Path $out 'item-description.png')
Get-ChildItem -LiteralPath $out | Select-Object Name,Length

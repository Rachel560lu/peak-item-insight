param(
    [string]$FfmpegPath,
    [string]$SourceDir = 'C:\Users\midor\Desktop',
    [switch]$RefreshLegacy
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
    @{ Input = '预览运动饮料.mp4'; Output = 'sports-drink.gif'; Start = '0'; Duration = '3.2' },
    @{ Input = '运动饮料：已有额外精力条.mp4'; Output = 'sports-drink-existing-bonus.gif'; Start = '0'; Duration = '4.8' },
    @{ Input = '急救箱预览.mp4'; Output = 'first-aid-kit.gif'; Start = '0'; Duration = '2.2' },
    @{ Input = '特殊道具功能介绍.mp4'; Output = 'special-item.gif'; Start = '0'; Duration = '0.8' },
    @{ Input = '屏幕录制 202灵药菇解释6-09-12 140004.mp4'; Output = 'remedy-fungus.gif'; Start = '0'; Duration = '2.8' }
)
if ($RefreshLegacy) {
    $clips += @(
        @{ Input = '能量饮料预览.mp4'; Output = 'energy-drink.gif'; Start = '3.8'; Duration = '6.0'; FrameRate = 8; Colors = 96 },
        @{ Input = '有毒蘑菇preview.mp4'; Output = 'poisonous-mushroom.gif'; Start = '0'; Duration = '2.8' },
        @{ Input = '梅子有毒.mp4'; Output = 'poisonous-food.gif'; Start = '0.4'; Duration = '5.3'; Colors = 96 },
        @{ Input = '蘑菇无毒预览.mp4'; Output = 'non-poisonous-food.gif'; Start = '0'; Duration = '2.3' },
        @{ Input = '物品简介.png'; Output = 'item-description.gif' }
    )
}
foreach ($clip in $clips) {
    if (!(Test-Path -LiteralPath (Join-Path $SourceDir $clip.Input))) { throw "Missing demonstration: $($clip.Input)" }
}
foreach ($clip in $clips) {
    $inputPath = Join-Path $SourceDir $clip.Input
    if (!(Test-Path -LiteralPath $inputPath)) { throw "Missing demonstration: $inputPath" }
    # Keep the whole HUD, original speed and chronological order; trim only the
    # non-demonstration/menu sections. No invented overlays; GIF has no audio.
    # Legacy energy-drink footage uses a lower frame rate/palette to bound size.
    $frameRate = if ($clip.FrameRate) { $clip.FrameRate } else { 10 }
    $colors = if ($clip.Colors) { $clip.Colors } else { 128 }
    $filter = "fps=$frameRate,scale=960:-1:flags=lanczos,setsar=1,split[a][b];[a]palettegen=max_colors=${colors}:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=5:diff_mode=rectangle"
    if ($clip.Input.EndsWith('.png')) { $filter = $filter.Replace("fps=$frameRate,", '') }
    $startArgs = @()
    if ($clip.Start) { $startArgs = @('-ss', $clip.Start) }
    $durationArgs = @()
    if ($clip.Duration) { $durationArgs = @('-t', $clip.Duration) }
    & $FfmpegPath -hide_banner -loglevel error -y @startArgs -i $inputPath @durationArgs -an -filter_complex $filter -loop 0 (Join-Path $out $clip.Output)
    if ($LASTEXITCODE -ne 0) { throw "GIF conversion failed: $inputPath" }
}
if ($RefreshLegacy) { Copy-Item -LiteralPath (Join-Path $SourceDir '物品简介.png') -Destination (Join-Path $out 'item-description.png') }
Get-ChildItem -LiteralPath $out | Select-Object Name,Length

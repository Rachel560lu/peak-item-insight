param([string]$FfmpegPath = 'C:\Users\midor\Desktop\peak-item-insight\.media-tools\imageio_ffmpeg\binaries\ffmpeg-win-x86_64-v7.1.exe')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
# Preserve original speed and full HUD; omit the pause-menu endings.
foreach ($clip in @(
    @{ Input = 'C:\Users\midor\Desktop\能量饮料运动饮料.mp4'; Duration = 4.5; Output = 'drinks-029.gif' },
    @{ Input = 'C:\Users\midor\Desktop\额外精力条预览坚果.mp4'; Duration = 3.5; Output = 'trail-mix-029.gif' }
)) {
    & $FfmpegPath -hide_banner -loglevel error -y -i $clip.Input -t $clip.Duration -an -filter_complex 'fps=10,scale=960:-1:flags=lanczos,setsar=1,split[a][b];[a]palettegen=max_colors=128:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=5:diff_mode=rectangle' -loop 0 (Join-Path $root ('docs/assets/demos/' + $clip.Output))
    if ($LASTEXITCODE -ne 0) { throw 'GIF conversion failed.' }
}

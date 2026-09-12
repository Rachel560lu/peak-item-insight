param([string]$FfmpegPath, [string]$InputPath = 'C:\Users\midor\Desktop\新能量饮料预览.mp4')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (!$FfmpegPath) { $FfmpegPath = (Get-ChildItem (Join-Path $root '.media-tools/imageio_ffmpeg/binaries') -Filter '*.exe' | Select-Object -First 1).FullName }
# Original speed and full HUD. Stop before the item is put away and pause menu opens.
& $FfmpegPath -hide_banner -loglevel error -y -i $InputPath -t 5 -an -filter_complex 'fps=10,scale=960:-1:flags=lanczos,setsar=1,split[a][b];[a]palettegen=max_colors=128:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=5:diff_mode=rectangle' -loop 0 (Join-Path $root 'docs/assets/demos/gameplay-028.gif')
if ($LASTEXITCODE -ne 0) { throw 'Gameplay GIF conversion failed.' }

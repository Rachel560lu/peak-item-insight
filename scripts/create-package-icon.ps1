$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$output = Join-Path (Split-Path -Parent $PSScriptRoot) 'thunderstore/icon.png'
# Original geometric artwork; no game logo, screenshot or third-party image.
$bitmap = [System.Drawing.Bitmap]::new(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$resources = [System.Collections.Generic.List[System.IDisposable]]::new()
function Brush([string]$hex) {
    $value = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($hex))
    $resources.Add($value)
    return $value
}
try {
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#12252D'))
    $mountain = Brush '#355963'
    $snow = Brush '#D9EEE5'
    $green = Brush '#83DC69'
    $yellow = Brush '#E9BA50'
    $orange = Brush '#E8764B'
    $graphics.FillPolygon($mountain, [System.Drawing.Point[]]@(
        [System.Drawing.Point]::new(28,177), [System.Drawing.Point]::new(111,55),
        [System.Drawing.Point]::new(150,113), [System.Drawing.Point]::new(174,83),
        [System.Drawing.Point]::new(229,177)))
    $graphics.FillPolygon($snow, [System.Drawing.Point[]]@(
        [System.Drawing.Point]::new(86,92), [System.Drawing.Point]::new(111,55),
        [System.Drawing.Point]::new(138,95), [System.Drawing.Point]::new(113,85),
        [System.Drawing.Point]::new(102,97)))
    $pen = [System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml('#83DC69'), 5)
    $resources.Add($pen)
    $graphics.DrawEllipse($pen, 87, 99, 82, 82)
    $graphics.DrawLine($pen, 128, 88, 128, 111)
    $graphics.DrawLine($pen, 128, 169, 128, 192)
    $graphics.DrawLine($pen, 76, 140, 99, 140)
    $graphics.DrawLine($pen, 157, 140, 180, 140)
    $graphics.FillEllipse($snow, 123, 135, 10, 10)
    $frame = [System.Drawing.Pen]::new([System.Drawing.ColorTranslator]::FromHtml('#D9EEE5'), 3)
    $resources.Add($frame)
    $graphics.DrawRectangle($frame, 27, 209, 202, 22)
    $graphics.FillRectangle($green, 32, 214, 135, 12)
    $graphics.FillRectangle($yellow, 171, 214, 27, 12)
    $graphics.FillRectangle($orange, 202, 214, 22, 12)
    $bitmap.Save($output, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Output "Icon generated: $output (256x256 PNG)"
} finally {
    foreach ($resource in $resources) { $resource.Dispose() }
    $graphics.Dispose()
    $bitmap.Dispose()
}

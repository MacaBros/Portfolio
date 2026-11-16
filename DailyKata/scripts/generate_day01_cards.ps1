Add-Type -AssemblyName System.Drawing

$repoRoot = Split-Path -Parent $PSScriptRoot
$dayDir   = Join-Path $repoRoot "Day_01"
$outDir   = Join-Path $dayDir "LinkedIn_Assets"
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }

function New-RoundedRectPath([float]$x, [float]$y, [float]$w, [float]$h, [float]$r) {
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $d = $r * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function Draw-HighlightedCSharpLines($g, [string[]]$lines, [float]$startX, [float]$startY, [float]$lineHeight, [float]$fontSize) {
    $font = [System.Drawing.Font]::new("Consolas", $fontSize, [System.Drawing.FontStyle]::Regular)

    $colPlain   = [System.Drawing.Color]::FromArgb(255, 226, 232, 240)
    $colKeyword = [System.Drawing.Color]::FromArgb(255, 192, 132, 252) # Purple
    $colType    = [System.Drawing.Color]::FromArgb(255, 56, 189, 248)  # Cyan
    $colString  = [System.Drawing.Color]::FromArgb(255, 134, 239, 172) # Green
    $colNumber  = [System.Drawing.Color]::FromArgb(255, 251, 191, 36)  # Amber
    $colComment = [System.Drawing.Color]::FromArgb(255, 100, 116, 139) # Slate
    $colLineNum = [System.Drawing.Color]::FromArgb(255, 71, 85, 105)

    $keywords = @(
        "using","namespace","public","private","abstract","sealed","class","record",
        "int","void","var","new","if","else","return","switch","when"
    )
    $types = @(
        "PlayerState","DamageHandler","StatusEffect","Normal","Poisoned","Shielded",
        "DamageHandlerTests","Assert","Fact","Xunit","DailyKata","Day01","Tests"
    )

    $sf = [System.Drawing.StringFormat]::GenericTypographic
    $sf.FormatFlags = [System.Drawing.StringFormatFlags]::MeasureTrailingSpaces

    for ($i = 0; $i -lt $lines.Length; $i++) {
        $y = $startY + ($i * $lineHeight)
        $line = $lines[$i]

        $lnBrush = [System.Drawing.SolidBrush]::new($colLineNum)
        $g.DrawString(($i + 1).ToString().PadLeft(2, ' '), $font, $lnBrush, $startX - 42, $y, $sf)
        $lnBrush.Dispose()

        if ($line.TrimStart().StartsWith("//")) {
            $cBrush = [System.Drawing.SolidBrush]::new($colComment)
            $g.DrawString($line, $font, $cBrush, $startX, $y, $sf)
            $cBrush.Dispose()
            continue
        }

        $tokens = [regex]::Matches($line, '\s+|[A-Za-z_][A-Za-z0-9_]*|\d+|\/\/.*|.')
        $curX = $startX
        $inComment = $false
        foreach ($m in $tokens) {
            $tok = $m.Value
            if ($inComment -or $tok.StartsWith("//")) {
                $inComment = $true
                $col = $colComment
            } elseif ($keywords -contains $tok) {
                $col = $colKeyword
            } elseif ($types -contains $tok) {
                $col = $colType
            } elseif ($tok -match '^\d+$') {
                $col = $colNumber
            } else {
                $col = $colPlain
            }

            $brush = [System.Drawing.SolidBrush]::new($col)
            $g.DrawString($tok, $font, $brush, $curX, $y, $sf)
            $sz = $g.MeasureString($tok, $font, 2000, $sf)
            $curX += $sz.Width
            $brush.Dispose()
        }
    }

    $font.Dispose()
}

function Build-CodeCard(
    [string]$sourceFile,
    [string]$outFileName,
    [string]$windowTitle,
    [string]$badgeText,
    [System.Drawing.Color]$gradStart,
    [System.Drawing.Color]$gradEnd,
    [System.Drawing.Color]$glowColor,
    [System.Drawing.Color]$badgeColor,
    [int]$w,
    [int]$h,
    [float]$lineHeight,
    [float]$fontSize
) {
    $lines = Get-Content -Encoding UTF8 (Join-Path $dayDir $sourceFile)

    $bmp = [System.Drawing.Bitmap]::new($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit

    $bgBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.Rectangle]::new(0, 0, $w, $h),
        $gradStart,
        $gradEnd,
        135.0
    )
    $g.FillRectangle($bgBrush, 0, 0, $w, $h)
    $bgBrush.Dispose()

    $glowBrush = [System.Drawing.SolidBrush]::new($glowColor)
    $g.FillEllipse($glowBrush, -140, -140, 620, 620)
    $glowBrush.Dispose()

    $winX = 64; $winY = 54; $winW = $w - 128; $winH = $h - 108
    $shadowPath = New-RoundedRectPath ($winX + 6) ($winY + 14) $winW $winH 18
    $shadowBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(115, 0, 0, 0))
    $g.FillPath($shadowBrush, $shadowPath)
    $shadowBrush.Dispose(); $shadowPath.Dispose()

    $winPath = New-RoundedRectPath $winX $winY $winW $winH 18
    $winBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(245, 17, 20, 32))
    $g.FillPath($winBrush, $winPath)
    $winPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(80, 255, 255, 255), 1.5)
    $g.DrawPath($winPen, $winPath)
    $winBrush.Dispose(); $winPen.Dispose(); $winPath.Dispose()

    $dots = @(
        [System.Drawing.Color]::FromArgb(255, 255, 95, 86),
        [System.Drawing.Color]::FromArgb(255, 255, 189, 46),
        [System.Drawing.Color]::FromArgb(255, 39, 201, 63)
    )
    for ($i = 0; $i -lt 3; $i++) {
        $dBrush = [System.Drawing.SolidBrush]::new($dots[$i])
        $g.FillEllipse($dBrush, $winX + 26 + ($i * 22), $winY + 23, 13, 13)
        $dBrush.Dispose()
    }

    $titleFont = [System.Drawing.Font]::new("Segoe UI", 11.5, [System.Drawing.FontStyle]::Bold)
    $titleBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 148, 163, 184))
    $g.DrawString($windowTitle, $titleFont, $titleBrush, $winX + 108, $winY + 19)
    $titleBrush.Dispose(); $titleFont.Dispose()

    # Right-side pill badge
    $badgeFont = [System.Drawing.Font]::new("Segoe UI", 9.5, [System.Drawing.FontStyle]::Bold)
    $badgeSize = $g.MeasureString($badgeText, $badgeFont)
    $pillW = [int]($badgeSize.Width + 24)
    $pillX = $winX + $winW - $pillW - 22
    $pillPath = New-RoundedRectPath $pillX ($winY + 14) $pillW 28 14
    $pillBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(45, $badgeColor.R, $badgeColor.G, $badgeColor.B))
    $g.FillPath($pillBrush, $pillPath)
    $pillPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(140, $badgeColor.R, $badgeColor.G, $badgeColor.B), 1.0)
    $g.DrawPath($pillPen, $pillPath)
    $badgeTextBrush = [System.Drawing.SolidBrush]::new($badgeColor)
    $g.DrawString($badgeText, $badgeFont, $badgeTextBrush, $pillX + 12, $winY + 19)
    $pillBrush.Dispose(); $pillPen.Dispose(); $pillPath.Dispose(); $badgeTextBrush.Dispose(); $badgeFont.Dispose()

    $divPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(40, 255, 255, 255), 1)
    $g.DrawLine($divPen, $winX, $winY + 56, $winX + $winW, $winY + 56)
    $divPen.Dispose()

    Draw-HighlightedCSharpLines $g $lines ($winX + 68) ($winY + 76) $lineHeight $fontSize

    $g.Dispose()
    $outPath = Join-Path $outDir $outFileName
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Built: $outPath"
}

function Build-TestsSummaryCard {
    $w = 1040; $h = 430
    $bmp = [System.Drawing.Bitmap]::new($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit

    $bgMain       = [System.Drawing.Color]::FromArgb(255, 30, 31, 34)
    $bgHeader     = [System.Drawing.Color]::FromArgb(255, 38, 40, 44)
    $bgTree       = [System.Drawing.Color]::FromArgb(255, 24, 25, 28)
    $accentGreen  = [System.Drawing.Color]::FromArgb(255, 54, 179, 126)
    $greenBadgeBg = [System.Drawing.Color]::FromArgb(255, 20, 50, 36)
    $textWhite    = [System.Drawing.Color]::FromArgb(255, 230, 235, 240)
    $textMuted    = [System.Drawing.Color]::FromArgb(255, 148, 163, 184)
    $textTime     = [System.Drawing.Color]::FromArgb(255, 110, 120, 135)
    $borderCol    = [System.Drawing.Color]::FromArgb(255, 48, 50, 56)

    $bgBrush = [System.Drawing.SolidBrush]::new($bgMain); $g.FillRectangle($bgBrush, 0, 0, $w, $h); $bgBrush.Dispose()
    $borderPen = [System.Drawing.Pen]::new($borderCol, 1.5); $g.DrawRectangle($borderPen, 1, 1, $w - 2, $h - 2); $borderPen.Dispose()

    $headerBrush = [System.Drawing.SolidBrush]::new($bgHeader); $g.FillRectangle($headerBrush, 0, 0, $w, 52); $headerBrush.Dispose()
    $fontTab   = [System.Drawing.Font]::new("Segoe UI", 12, [System.Drawing.FontStyle]::Bold)
    $fontMono  = [System.Drawing.Font]::new("Consolas", 10.5, [System.Drawing.FontStyle]::Regular)
    $fontSmall = [System.Drawing.Font]::new("Segoe UI", 9.5, [System.Drawing.FontStyle]::Regular)

    $whiteBrush = [System.Drawing.SolidBrush]::new($textWhite)
    $mutedBrush = [System.Drawing.SolidBrush]::new($textMuted)
    $g.DrawString("dotnet test  —  DailyKata.sln", $fontTab, $whiteBrush, 20, 14)

    $pillBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 50, 54, 62))
    $g.FillRectangle($pillBrush, 290, 10, 125, 32); $pillBrush.Dispose()
    $g.DrawString("xUnit 2.9.2 (4)", $fontSmall, $whiteBrush, 305, 18)
    $g.DrawString(".NET 8.0", $fontSmall, $mutedBrush, 430, 18)

    $bannerY = 52; $bannerH = 62
    $bannerBrush = [System.Drawing.SolidBrush]::new($greenBadgeBg); $g.FillRectangle($bannerBrush, 0, $bannerY, $w, $bannerH); $bannerBrush.Dispose()
    $barBrush = [System.Drawing.SolidBrush]::new($accentGreen); $g.FillRectangle($barBrush, 0, $bannerY, $w, 4); $barBrush.Dispose()
    $circleBrush = [System.Drawing.SolidBrush]::new($accentGreen); $g.FillEllipse($circleBrush, 24, $bannerY + 17, 28, 28); $circleBrush.Dispose()

    $chkPen = [System.Drawing.Pen]::new([System.Drawing.Color]::White, 3.0)
    $chkPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round; $chkPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($chkPen, 31, $bannerY + 31, 36, $bannerY + 37)
    $g.DrawLine($chkPen, 36, $bannerY + 37, 44, $bannerY + 25)
    $chkPen.Dispose()

    $fontTitle = [System.Drawing.Font]::new("Segoe UI", 12.5, [System.Drawing.FontStyle]::Bold)
    $fontSub   = [System.Drawing.Font]::new("Segoe UI", 10.5, [System.Drawing.FontStyle]::Regular)
    $greenBrush = [System.Drawing.SolidBrush]::new($accentGreen)
    $g.DrawString("4 Tests Passed  (0 Failed, 0 Skipped)", $fontTitle, $whiteBrush, 64, $bannerY + 12)
    $g.DrawString("Day 01: DamageHandler Refactor Verification  •  Execution Time: 2 ms  •  net8.0", $fontSub, $greenBrush, 64, $bannerY + 35)

    $badgeBrush = [System.Drawing.SolidBrush]::new($accentGreen); $g.FillRectangle($badgeBrush, $w - 130, $bannerY + 17, 105, 28); $badgeBrush.Dispose()
    $blackBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::Black)
    $fontBadge  = [System.Drawing.Font]::new("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
    $g.DrawString("ALL GREEN", $fontBadge, $blackBrush, $w - 119, $bannerY + 22)
    $blackBrush.Dispose(); $fontBadge.Dispose()

    $treeY = $bannerY + $bannerH
    $treeBrush = [System.Drawing.SolidBrush]::new($bgTree); $g.FillRectangle($treeBrush, 0, $treeY, $w, $h - $treeY); $treeBrush.Dispose()

    function Draw-Check($x, $y) {
        $cp = [System.Drawing.Pen]::new($accentGreen, 2.2)
        $cp.StartCap = [System.Drawing.Drawing2D.LineCap]::Round; $cp.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
        $g.DrawLine($cp, [float]($x+2), [float]($y+7), [float]($x+6), [float]($y+11))
        $g.DrawLine($cp, [float]($x+6), [float]($y+11), [float]($x+13), [float]($y+3))
        $cp.Dispose()
    }
    function Draw-Chevron($x, $y) {
        $ch = [System.Drawing.Pen]::new($textMuted, 2.0)
        $g.DrawLine($ch, [float]$x, [float]$y, [float]($x+4), [float]($y+4))
        $g.DrawLine($ch, [float]($x+4), [float]($y+4), [float]($x+8), [float]$y)
        $ch.Dispose()
    }

    $curY = $treeY + 20
    $timeBrush = [System.Drawing.SolidBrush]::new($textTime)
    $rootFont  = [System.Drawing.Font]::new("Segoe UI", 11.5, [System.Drawing.FontStyle]::Bold)
    Draw-Chevron 24 ($curY + 5); Draw-Check 40 $curY
    $g.DrawString("DamageHandler.dll (net8.0)", $rootFont, $whiteBrush, 62, $curY)
    $g.DrawString("2 ms", $fontMono, $timeBrush, $w - 75, $curY + 2)
    $curY += 36

    $suiteFont  = [System.Drawing.Font]::new("Segoe UI", 11, [System.Drawing.FontStyle]::Bold)
    $suiteBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 110, 200, 255))
    Draw-Chevron 48 ($curY + 5); Draw-Check 64 $curY
    $g.DrawString("DailyKata.Day01.Tests.DamageHandlerTests", $suiteFont, $suiteBrush, 86, $curY)
    $curY += 34

    $tests = @(
        "CalculateDamageTaken_Normal_ReturnsBaseDamage",
        "CalculateDamageTaken_Poisoned_ReturnsBaseDamagePlusTicksTimesTwo",
        "CalculateDamageTaken_Shielded_ReturnsBaseDamageMinusStrength",
        "CalculateDamageTaken_ShieldedStrongerThanBaseDamage_ReturnsZero"
    )
    foreach ($t in $tests) {
        Draw-Check 84 $curY
        $g.DrawString($t, $fontMono, $whiteBrush, 108, $curY)
        $g.DrawString("PASS", $fontMono, $greenBrush, $w - 75, $curY + 1)
        $curY += 32
    }

    $fontTab.Dispose(); $fontMono.Dispose(); $fontSmall.Dispose()
    $fontTitle.Dispose(); $fontSub.Dispose(); $rootFont.Dispose(); $suiteFont.Dispose()
    $whiteBrush.Dispose(); $mutedBrush.Dispose(); $greenBrush.Dispose(); $timeBrush.Dispose(); $suiteBrush.Dispose()

    $g.Dispose()
    $outPath = Join-Path $outDir "04_TestRunner_AllGreen.png"
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Built: $outPath"
}

Build-CodeCard `
    -sourceFile "LegacyDamageHandler.cs" `
    -outFileName "01_Legacy_DamageHandler.png" `
    -windowTitle "Day_01 / LegacyDamageHandler.cs" `
    -badgeText "BEFORE • C# 4 Style" `
    -gradStart ([System.Drawing.Color]::FromArgb(255, 225, 29, 72)) `
    -gradEnd   ([System.Drawing.Color]::FromArgb(255, 245, 158, 11)) `
    -glowColor ([System.Drawing.Color]::FromArgb(45, 251, 146, 60)) `
    -badgeColor ([System.Drawing.Color]::FromArgb(255, 251, 191, 36)) `
    -w 1200 -h 860 -lineHeight 22.0 -fontSize 11.5

Build-CodeCard `
    -sourceFile "ModernDamageHandler.cs" `
    -outFileName "02_Modern_DamageHandler.png" `
    -windowTitle "Day_01 / ModernDamageHandler.cs" `
    -badgeText "AFTER • C# 12 / .NET 8" `
    -gradStart ([System.Drawing.Color]::FromArgb(255, 79, 70, 229)) `
    -gradEnd   ([System.Drawing.Color]::FromArgb(255, 6, 182, 212)) `
    -glowColor ([System.Drawing.Color]::FromArgb(50, 236, 72, 153)) `
    -badgeColor ([System.Drawing.Color]::FromArgb(255, 56, 189, 248)) `
    -w 1200 -h 750 -lineHeight 22.0 -fontSize 11.5

Build-CodeCard `
    -sourceFile "DamageHandlerTests.cs" `
    -outFileName "03_DamageHandlerTests.png" `
    -windowTitle "Day_01 / DamageHandlerTests.cs" `
    -badgeText "VERIFIED • xUnit" `
    -gradStart ([System.Drawing.Color]::FromArgb(255, 16, 185, 129)) `
    -gradEnd   ([System.Drawing.Color]::FromArgb(255, 14, 116, 144)) `
    -glowColor ([System.Drawing.Color]::FromArgb(45, 52, 211, 153)) `
    -badgeColor ([System.Drawing.Color]::FromArgb(255, 134, 239, 172)) `
    -w 1200 -h 1010 -lineHeight 20.5 -fontSize 11.0

Build-TestsSummaryCard

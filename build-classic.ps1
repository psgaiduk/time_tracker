param(
    [switch]$Test
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sourceRoot = Join-Path $projectRoot 'classic'
$outputDirectory = Join-Path $projectRoot 'artifacts\classic'
$outputName = if ($Test) { 'TimeTracker-Test.exe' } else { 'TimeTracker.exe' }
$outputPath = Join-Path $outputDirectory $outputName
$sources = Get-ChildItem -LiteralPath $sourceRoot -Filter '*.cs' -Recurse | ForEach-Object { $_.FullName }

New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$englishResources = Join-Path $sourceRoot 'Presentation\StringsEn.resx'
$russianResources = Join-Path $sourceRoot 'Presentation\StringsRu.resx'
$arguments = @(
    '/nologo',
    '/target:winexe',
    '/optimize+',
    '/reference:System.dll',
    '/reference:System.Drawing.dll',
    '/reference:System.Windows.Forms.dll',
    '/reference:System.Xml.dll',
    "/out:$outputPath",
    "/resource:$englishResources,StringsEn.resx",
    "/resource:$russianResources,StringsRu.resx"
)
if ($Test) { $arguments += '/define:TEST_TIMER' }
$arguments += $sources

& $compiler $arguments
if ($LASTEXITCODE -ne 0) { throw "Classic build failed with exit code $LASTEXITCODE." }
Write-Host "Built: $outputPath"

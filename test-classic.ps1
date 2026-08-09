$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$outputDirectory = Join-Path $projectRoot 'artifacts\classic-tests'
$outputPath = Join-Path $outputDirectory 'TimeTracker.Classic.Tests.exe'
$sources = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'classic') -Filter '*.cs' -Recurse | ForEach-Object { $_.FullName })
$sources += Join-Path $projectRoot 'tests\TimeTracker.Classic.Tests\ClassicTimerTests.cs'

New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
& $compiler /nologo /target:exe /main:TimeTracker.Classic.Tests.ClassicTimerTests /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "/out:$outputPath" $sources
if ($LASTEXITCODE -ne 0) { throw "Classic tests failed to compile." }
& $outputPath
if ($LASTEXITCODE -ne 0) { throw "Classic tests failed." }

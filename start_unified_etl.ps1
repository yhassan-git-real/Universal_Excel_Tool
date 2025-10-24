# Universal Excel Tool - Unified ETL Process Launcher
Write-Host ""
Write-Host "================================================================"
Write-Host "         UNIVERSAL EXCEL TOOL - UNIFIED ETL LAUNCHER"
Write-Host "================================================================"
Write-Host ""

# Get the directory where this script is located
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Define the orchestrator path (try Self-Contained first, then Release, then Debug)
$orchestratorSelfContained = Join-Path $scriptDir "Core\bin\Release\net8.0\win-x64\publish\UniversalExcelTool.exe"
$orchestratorRelease = Join-Path $scriptDir "Core\bin\Release\net8.0\UniversalExcelTool.exe"
$orchestratorDebug = Join-Path $scriptDir "Core\bin\Debug\net8.0\UniversalExcelTool.exe"

if (Test-Path $orchestratorSelfContained) {
    $orchestrator = $orchestratorSelfContained
    $buildType = "Self-Contained"
} elseif (Test-Path $orchestratorRelease) {
    $orchestrator = $orchestratorRelease
    $buildType = "Release"
} elseif (Test-Path $orchestratorDebug) {
    $orchestrator = $orchestratorDebug
    $buildType = "Debug"
} else {
    Write-Host "ERROR: Orchestrator executable not found in any configuration" -ForegroundColor Red
    Write-Host "Expected locations:" -ForegroundColor Yellow
    Write-Host "  Self-Contained: $orchestratorSelfContained" -ForegroundColor Yellow
    Write-Host "  Release: $orchestratorRelease" -ForegroundColor Yellow
    Write-Host "  Debug: $orchestratorDebug" -ForegroundColor Yellow
    Write-Host "Please build the project first:" -ForegroundColor Yellow
    Write-Host "  For self-contained: .\build_self_contained.bat" -ForegroundColor Yellow
    Write-Host "  For regular build: cd Core; dotnet build --configuration Release" -ForegroundColor Yellow
    Read-Host "Press Enter to continue"
    exit 1
}

Write-Host "Starting Universal Excel Tool ETL Process..." -ForegroundColor Green
Write-Host "Orchestrator ($buildType): $orchestrator" -ForegroundColor Cyan
Write-Host ""

# Execute the orchestrator
& $orchestrator

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
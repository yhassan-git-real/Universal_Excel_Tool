# Create Desktop Shortcut for Universal Excel Tool UI
# Run this script after building the self-contained application

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Creating Desktop Shortcut" -ForegroundColor Cyan
Write-Host "Universal Excel Tool UI" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Get the desktop path
$desktopPath = [Environment]::GetFolderPath("Desktop")

# Get the executable path
$exePath = Join-Path $PSScriptRoot "bin\Release\net8.0\win-x64\UniversalExcelTool.UI.exe"

# Check if executable exists
if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: Executable not found!" -ForegroundColor Red
    Write-Host "Please build the application first using build_self_contained.ps1" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Expected location: $exePath" -ForegroundColor Gray
    pause
    exit 1
}

# Create shortcut
$shortcutPath = Join-Path $desktopPath "Universal Excel Tool.lnk"

$WScriptShell = New-Object -ComObject WScript.Shell
$Shortcut = $WScriptShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath = $exePath
$Shortcut.WorkingDirectory = Split-Path $exePath
$Shortcut.Description = "Universal Excel Tool - Modern ETL Manager"
$Shortcut.WindowStyle = 1  # Normal window
$Shortcut.Save()

Write-Host "[SUCCESS] Desktop shortcut created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Shortcut location: $shortcutPath" -ForegroundColor White
Write-Host "Target: $exePath" -ForegroundColor Gray
Write-Host ""
Write-Host "You can now double-click the shortcut on your desktop to launch the application." -ForegroundColor Yellow
Write-Host ""
pause

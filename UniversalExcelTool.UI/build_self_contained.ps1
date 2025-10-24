# PowerShell Build Script for Universal Excel Tool UI
# Creates self-contained executable with all dependencies

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Building Universal Excel Tool UI" -ForegroundColor Cyan
Write-Host "Self-Contained Release Build" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Clean previous builds
Write-Host "[1/5] Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "bin\Release") {
    Remove-Item -Recurse -Force "bin\Release"
}
if (Test-Path "obj\Release") {
    Remove-Item -Recurse -Force "obj\Release"
}
Write-Host "Done." -ForegroundColor Green
Write-Host ""

# Build Core project first
Write-Host "[2/5] Building Core project..." -ForegroundColor Yellow
Push-Location ..\Core
dotnet build -c Release
Pop-Location
Write-Host "Done." -ForegroundColor Green
Write-Host ""

# Restore dependencies
Write-Host "[3/5] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
Write-Host "Done." -ForegroundColor Green
Write-Host ""

# Build self-contained for Windows x64
Write-Host "[4/5] Building self-contained application..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    -p:DebugSymbols=false
Write-Host "Done." -ForegroundColor Green
Write-Host ""

# Copy configuration files
Write-Host "[5/5] Copying configuration files..." -ForegroundColor Yellow
$publishPath = "bin\Release\net8.0\win-x64\publish"
if (Test-Path "..\..\appsettings.json") {
    Copy-Item "..\..\appsettings.json" -Destination $publishPath -Force
}
if (Test-Path "..\..\dynamic_table_config.json") {
    Copy-Item "..\..\dynamic_table_config.json" -Destination $publishPath -Force
}
Write-Host "Done." -ForegroundColor Green
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output location: $publishPath" -ForegroundColor White
Write-Host "Executable: UniversalExcelTool.UI.exe" -ForegroundColor White
Write-Host ""
Write-Host "You can now:" -ForegroundColor Yellow
Write-Host "1. Run the executable from the publish folder" -ForegroundColor White
Write-Host "2. Create a desktop shortcut to UniversalExcelTool.UI.exe" -ForegroundColor White
Write-Host "3. Copy the entire publish folder to another machine" -ForegroundColor White
Write-Host ""

# Ask if user wants to run the application
$response = Read-Host "Do you want to run the application now? (Y/N)"
if ($response -eq 'Y' -or $response -eq 'y') {
    Write-Host ""
    Write-Host "Launching Universal Excel Tool..." -ForegroundColor Cyan
    Start-Process "$publishPath\UniversalExcelTool.UI.exe"
}

@echo off
REM Build and Run Script for Universal Excel Tool UI

echo ================================================
echo Universal Excel Tool - Avalonia UI Builder
echo ================================================
echo.

REM Check if .NET 8.0 is installed
echo Checking .NET SDK version...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo .NET SDK found!
echo.

REM Navigate to UI project directory
cd /d "%~dp0"

echo ================================================
echo Step 1: Restoring NuGet packages...
echo ================================================
dotnet restore
if errorlevel 1 (
    echo ERROR: Package restore failed!
    pause
    exit /b 1
)
echo Packages restored successfully!
echo.

echo ================================================
echo Step 2: Building the project...
echo ================================================
dotnet build -c Release
if errorlevel 1 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo Build successful!
echo.

echo ================================================
echo Step 3: Running the application...
echo ================================================
echo.
dotnet run -c Release

pause

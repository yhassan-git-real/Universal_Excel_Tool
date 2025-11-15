@echo off
REM Universal Excel Tool - Build and Run UI Application

setlocal enabledelayedexpansion

echo.
echo ================================================================
echo          UNIVERSAL EXCEL TOOL - BUILD AND RUN UI
echo ================================================================
echo.

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

echo Step 1: Building Core module (Debug)...
dotnet build "%SCRIPT_DIR%Core\UniversalExcelTool.csproj" --configuration Debug

if %errorlevel% neq 0 (
    echo.
    echo Core module build failed!
    pause
    exit /b 1
)

echo.
echo Step 2: Building UI project (Release)...
dotnet build "%SCRIPT_DIR%UniversalExcelTool.UI\UniversalExcelTool.UI.csproj" --configuration Release

if %errorlevel% neq 0 (
    echo.
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Closing any running instances...
taskkill /F /IM UniversalExcelTool.UI.exe 2>nul

echo.
echo Build successful! Starting UI application...
echo.

REM Define the UI executable path - use the freshly built Release version
set "UI_EXE=%SCRIPT_DIR%UniversalExcelTool.UI\bin\Release\net8.0\UniversalExcelTool.UI.exe"

REM Check if UI executable exists
if not exist "%UI_EXE%" (
    echo ERROR: UI executable not found at: %UI_EXE%
    echo Please ensure the build completed successfully
    pause
    exit /b 1
)

echo Starting: %UI_EXE%
echo.

REM Run the UI application
start "" "%UI_EXE%"

pause
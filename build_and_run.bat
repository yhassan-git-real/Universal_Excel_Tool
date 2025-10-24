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

echo Building UI project...
dotnet build "%SCRIPT_DIR%UniversalExcelTool.UI\UniversalExcelTool.UI.csproj" --configuration Release

if %errorlevel% neq 0 (
    echo.
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Build successful! Starting UI application...
echo.

REM Define the UI executable path (try self-contained first, then Release)
set "UI_EXE=%SCRIPT_DIR%UniversalExcelTool.UI\bin\Release\net8.0\win-x64\publish\UniversalExcelTool.UI.exe"
if not exist "%UI_EXE%" (
    set "UI_EXE=%SCRIPT_DIR%UniversalExcelTool.UI\bin\Release\net8.0\win-x64\UniversalExcelTool.UI.exe"
)
if not exist "%UI_EXE%" (
    set "UI_EXE=%SCRIPT_DIR%UniversalExcelTool.UI\bin\Release\net8.0\UniversalExcelTool.UI.exe"
)

REM Check if UI executable exists
if not exist "%UI_EXE%" (
    echo ERROR: UI executable not found
    echo Please ensure the build completed successfully
    pause
    exit /b 1
)

REM Run the UI application
"%UI_EXE%"

pause
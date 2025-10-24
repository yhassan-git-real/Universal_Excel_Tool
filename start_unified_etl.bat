@echo off
REM Universal Excel Tool - Unified ETL Process Launcher (Direct)

setlocal enabledelayedexpansion

echo.
echo ================================================================
echo          UNIVERSAL EXCEL TOOL - UNIFIED ETL LAUNCHER
echo ================================================================
echo.

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

REM Define the orchestrator path (try self-contained first, then Release, then Debug)
set "ORCHESTRATOR=%SCRIPT_DIR%Core\bin\Release\net8.0\win-x64\publish\UniversalExcelTool.exe"
if not exist "%ORCHESTRATOR%" (
    set "ORCHESTRATOR=%SCRIPT_DIR%Core\bin\Release\net8.0\UniversalExcelTool.exe"
)
if not exist "%ORCHESTRATOR%" (
    set "ORCHESTRATOR=%SCRIPT_DIR%Core\bin\Debug\net8.0\UniversalExcelTool.exe"
)

REM Check if orchestrator executable exists
if not exist "%ORCHESTRATOR%" (
    echo ERROR: Orchestrator executable not found in Release or Debug configurations
    echo Please build the Core project first:
    echo   cd Core
    echo   dotnet build --configuration Release
    pause
    exit /b 1
)

echo Starting Universal Excel Tool Orchestrator...
echo.

REM Execute the orchestrator directly with arguments
"%ORCHESTRATOR%" %*

REM Capture the exit code
set EXIT_CODE=%ERRORLEVEL%

echo.
if %EXIT_CODE% equ 0 (
    echo Process completed successfully
) else (
    echo Process completed with errors (Exit Code: %EXIT_CODE%^)
)

echo.
echo Press any key to exit...
pause >nul

exit /b %EXIT_CODE%
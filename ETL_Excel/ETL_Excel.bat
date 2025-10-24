@echo off
title Excel Processor - Universal Excel Tool
echo.
echo ===============================================================
echo                      EXCEL PROCESSOR
echo                Universal Excel Tool v2.0.0
echo ===============================================================
echo.

:: Set working directory to the main project root
cd /d "%~dp0.."

:: Check if executable exists (try self-contained first, then Release, then Debug)
if exist "ETL_Excel\bin\Release\net8.0\win-x64\publish\ETL_Excel.exe" (
    set "EXCEL_EXE=ETL_Excel\bin\Release\net8.0\win-x64\publish\ETL_Excel.exe"
    set "BUILD_TYPE=Self-Contained"
) else if exist "ETL_Excel\bin\Release\net8.0\ETL_Excel.exe" (
    set "EXCEL_EXE=ETL_Excel\bin\Release\net8.0\ETL_Excel.exe"
    set "BUILD_TYPE=Release"
) else if exist "ETL_Excel\bin\Debug\net8.0\ETL_Excel.exe" (
    set "EXCEL_EXE=ETL_Excel\bin\Debug\net8.0\ETL_Excel.exe"
    set "BUILD_TYPE=Debug"
) else (
    echo [ERROR] Application not found in any configuration!
    echo Expected locations:
    echo   Self-Contained: ETL_Excel\bin\Release\net8.0\win-x64\publish\ETL_Excel.exe
    echo   Release: ETL_Excel\bin\Release\net8.0\ETL_Excel.exe
    echo   Debug: ETL_Excel\bin\Debug\net8.0\ETL_Excel.exe
    echo.
    echo Please build the project first:
    echo   For self-contained: .\build_self_contained.bat
    echo   For regular build: dotnet build ETL_Excel/ETL_Excel.csproj -c Release
    echo.
    pause
    exit /b 1
)

echo [INFO] Starting Excel Processor (%BUILD_TYPE%)...
echo [INFO] Working Directory: %cd%
echo [INFO] Executable: %EXCEL_EXE%
echo [INFO] Processing and splitting Excel files into individual sheets
echo [INFO] Separating regular sheets and special sheets (SUP, DEM)
echo.

:: Show current folder structure
echo [INFO] Current Folder Structure:
echo         Files\                     (Raw Excel files)
echo         Files\ExcelFiles\          (Regular processed sheets)
echo         Files\Special_Sheets_Excels\ (Special sheets)
echo         Logs\                      (Application logs)
echo.

:: Ask user for execution mode
echo Choose execution mode:
echo [1] Interactive mode (default)
echo [2] Non-interactive mode (automated)
echo.
set /p choice="Enter your choice [1]: "
if "%choice%"=="" set choice=1

if "%choice%"=="2" (
    echo Running in non-interactive mode...
    "%EXCEL_EXE%" --non-interactive
) else (
    echo Running in interactive mode...
    "%EXCEL_EXE%"
)

echo.
if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Excel Processor completed successfully!
    echo.
    echo [INFO] Check the following folders for results:
    echo         Files\ExcelFiles\          - Regular processed sheets
    echo         Files\Special_Sheets_Excels\ - Special sheets (SUP, DEM)
    echo         Logs\                      - Processing logs
) else (
    echo [ERROR] Excel Processor failed with error code: %ERRORLEVEL%
)

echo.
pause
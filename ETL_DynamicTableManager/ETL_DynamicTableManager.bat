@echo off
title Dynamic Table Manager - Universal Excel Tool
echo.
echo ===============================================================
echo                   DYNAMIC TABLE MANAGER
echo                Universal Excel Tool v2.0.0
echo ===============================================================
echo.

:: Set working directory to the main project root
cd /d "%~dp0.."

:: Check if executable exists (try self-contained first, then Release, then Debug)
if exist "ETL_DynamicTableManager\bin\Release\net8.0\win-x64\publish\ETL_DynamicTableManager.exe" (
    set "DTM_EXE=ETL_DynamicTableManager\bin\Release\net8.0\win-x64\publish\ETL_DynamicTableManager.exe"
    set "BUILD_TYPE=Self-Contained"
) else if exist "ETL_DynamicTableManager\bin\Release\net8.0\ETL_DynamicTableManager.exe" (
    set "DTM_EXE=ETL_DynamicTableManager\bin\Release\net8.0\ETL_DynamicTableManager.exe"
    set "BUILD_TYPE=Release"
) else if exist "ETL_DynamicTableManager\bin\Debug\net8.0\ETL_DynamicTableManager.exe" (
    set "DTM_EXE=ETL_DynamicTableManager\bin\Debug\net8.0\ETL_DynamicTableManager.exe"
    set "BUILD_TYPE=Debug"
) else (
    echo [ERROR] Application not found in any configuration!
    echo Expected locations:
    echo   Self-Contained: ETL_DynamicTableManager\bin\Release\net8.0\win-x64\publish\ETL_DynamicTableManager.exe
    echo   Release: ETL_DynamicTableManager\bin\Release\net8.0\ETL_DynamicTableManager.exe
    echo   Debug: ETL_DynamicTableManager\bin\Debug\net8.0\ETL_DynamicTableManager.exe
    echo.
    echo Please build the project first:
    echo   For self-contained: .\build_self_contained.bat
    echo   For regular build: dotnet build ETL_DynamicTableManager/ETL_DynamicTableManager.csproj -c Release
    echo.
    pause
    exit /b 1
)

echo [INFO] Starting Dynamic Table Manager (%BUILD_TYPE%)...
echo [INFO] Working Directory: %cd%
echo [INFO] Executable: %DTM_EXE%
echo [INFO] Configuring dynamic table names for ETL process
echo.

:: Run the Dynamic Table Manager
"%DTM_EXE%"

echo.
if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Dynamic Table Manager completed successfully!
    echo.
    echo [INFO] Configuration created: dynamic_table_config.json
    echo [INFO] You can now run the Excel Processor or Database Loader
    echo         Next step: ETL_Excel\ETL_Excel.bat
) else (
    echo [ERROR] Dynamic Table Manager failed with error code: %ERRORLEVEL%
    echo.
    echo [INFO] Check the console output above for error details
    echo         Common issues: Missing Excel files, invalid file format
)

echo.
pause
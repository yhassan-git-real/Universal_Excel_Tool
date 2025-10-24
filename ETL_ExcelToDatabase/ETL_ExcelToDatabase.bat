@echo off
title Database Loader - Universal Excel Tool
echo.
echo ===============================================================
echo                       DATABASE LOADER
echo                Universal Excel Tool v2.0.0
echo ===============================================================
echo.

:: Set working directory to the main project root
cd /d "%~dp0.."

:: Check if executable exists (try self-contained first, then Release, then Debug)
if exist "ETL_ExcelToDatabase\bin\Release\net8.0\win-x64\publish\ETL_ExcelToDatabase.exe" (
    set "DB_EXE=ETL_ExcelToDatabase\bin\Release\net8.0\win-x64\publish\ETL_ExcelToDatabase.exe"
    set "BUILD_TYPE=Self-Contained"
) else if exist "ETL_ExcelToDatabase\bin\Release\net8.0\ETL_ExcelToDatabase.exe" (
    set "DB_EXE=ETL_ExcelToDatabase\bin\Release\net8.0\ETL_ExcelToDatabase.exe"
    set "BUILD_TYPE=Release"
) else if exist "ETL_ExcelToDatabase\bin\Debug\net8.0\ETL_ExcelToDatabase.exe" (
    set "DB_EXE=ETL_ExcelToDatabase\bin\Debug\net8.0\ETL_ExcelToDatabase.exe"
    set "BUILD_TYPE=Debug"
) else (
    echo [ERROR] Application not found in any configuration!
    echo Expected locations:
    echo   Self-Contained: ETL_ExcelToDatabase\bin\Release\net8.0\win-x64\publish\ETL_ExcelToDatabase.exe
    echo   Release: ETL_ExcelToDatabase\bin\Release\net8.0\ETL_ExcelToDatabase.exe
    echo   Debug: ETL_ExcelToDatabase\bin\Debug\net8.0\ETL_ExcelToDatabase.exe
    echo.
    echo Please build the project first:
    echo   For self-contained: .\build_self_contained.bat
    echo   For regular build: dotnet build ETL_ExcelToDatabase/ETL_ExcelToDatabase.csproj -c Release
    echo.
    pause
    exit /b 1
)

:: Check if dynamic table configuration exists
if not exist "dynamic_table_config.json" (
    echo [WARNING] Dynamic table configuration not found!
    echo.
    echo The Database Loader requires dynamic table configuration.
    echo Please run the Dynamic Table Manager first:
    echo   1. Run: ETL_DynamicTableManager\ETL_DynamicTableManager.bat
    echo   2. Or use the complete ETL process: start_unified_etl.bat
    echo.
    set /p continue="Do you want to continue anyway? [y/N]: "
    if /i not "%continue%"=="y" (
        echo Process cancelled.
        pause
        exit /b 1
    )
)

echo [INFO] Starting Database Loader (%BUILD_TYPE%)...
echo [INFO] Working Directory: %cd%
echo [INFO] Executable: %DB_EXE%
echo [INFO] Loading processed Excel data into database
echo [INFO] Server: MATRIX\MATRIX, Database: RAW_PROCESS
echo.

:: Show configuration info if available
if exist "dynamic_table_config.json" (
    echo [INFO] Dynamic Configuration Found:
    echo        Using dynamic table names from configuration
) else (
    echo [INFO] Static Configuration:
    echo        Using default table names from appsettings.json
)
echo.

:: Run the Database Loader
"%DB_EXE%"

echo.
if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Database Loader completed successfully!
    echo.
    echo [INFO] Check the following for results:
    echo        - Database tables: temp01, test01 (or configured names)
    echo        - Error logs: SELECT * FROM IMP_example_ERROR
    echo        - Success logs: SELECT * FROM IMP_example_SUCCESS
    echo        - Application logs: Logs\ folder
) else (
    echo [ERROR] Database Loader failed with error code: %ERRORLEVEL%
    echo.
    echo [INFO] Troubleshooting tips:
    echo    1. Ensure database server is running (MATRIX\MATRIX)
    echo    2. Check if dynamic table configuration exists
    echo    3. Verify Excel files exist in Files\ExcelFiles\
    echo    4. Check logs in Logs\ folder for detailed errors
)

echo.
pause
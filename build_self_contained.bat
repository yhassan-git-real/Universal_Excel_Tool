@echo off
title Universal Excel Tool - Self-Contained Build
echo.
echo ================================================================
echo      UNIVERSAL EXCEL TOOL - SELF-CONTAINED BUILD SCRIPT
echo                     Windows x64 Deployment
echo ================================================================
echo.

:: Set working directory to the main project root
cd /d "%~dp0"

echo [INFO] Building self-contained applications for Windows x64...
echo [INFO] This will create standalone executables that don't require .NET runtime
echo.

:: Clean previous builds
echo [INFO] Cleaning previous builds...
echo.

:: Build Core orchestrator
echo [INFO] Building Core orchestrator (UniversalExcelTool.exe)...
cd Core
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Core build failed!
    cd ..
    pause
    exit /b 1
)
cd ..
echo [SUCCESS] Core orchestrator built successfully!
echo.

:: Build ETL_Excel
echo [INFO] Building ETL_Excel (Excel Processor)...
cd ETL_Excel
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] ETL_Excel build failed!
    cd ..
    pause
    exit /b 1
)
cd ..
echo [SUCCESS] ETL_Excel built successfully!
echo.

:: Build ETL_ExcelToDatabase
echo [INFO] Building ETL_ExcelToDatabase (Database Loader)...
cd ETL_ExcelToDatabase
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] ETL_ExcelToDatabase build failed!
    cd ..
    pause
    exit /b 1
)
cd ..
echo [SUCCESS] ETL_ExcelToDatabase built successfully!
echo.

:: Build ETL_DynamicTableManager
echo [INFO] Building ETL_DynamicTableManager (Dynamic Table Manager)...
cd ETL_DynamicTableManager
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] ETL_DynamicTableManager build failed!
    cd ..
    pause
    exit /b 1
)
cd ..
echo [SUCCESS] ETL_DynamicTableManager built successfully!
echo.

:: Build ETL_CsvToDatabase
echo [INFO] Building ETL_CsvToDatabase (CSV Loader)...
cd ETL_CsvToDatabase
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] ETL_CsvToDatabase build failed!
    cd ..
    pause
    exit /b 1
)
cd ..
echo [SUCCESS] ETL_CsvToDatabase built successfully!
echo.

:: Build UniversalExcelTool.UI
echo [INFO] Building UniversalExcelTool.UI (Modern UI Application)...
cd UniversalExcelTool.UI
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] UniversalExcelTool.UI build failed!
    cd ..
    pause
    exit /b 1
)
cd ..
echo [SUCCESS] UniversalExcelTool.UI built successfully!
echo.

echo ================================================================
echo                    BUILD COMPLETED SUCCESSFULLY
echo ================================================================
echo.
echo [INFO] Self-contained executables created in:
echo         Core\bin\Release\net8.0\win-x64\publish\UniversalExcelTool.exe
echo         ETL_Excel\bin\Release\net8.0\win-x64\publish\ETL_Excel.exe
echo         ETL_ExcelToDatabase\bin\Release\net8.0\win-x64\publish\ETL_ExcelToDatabase.exe
echo         ETL_CsvToDatabase\bin\Release\net8.0\win-x64\publish\ETL_CsvToDatabase.exe
echo         ETL_DynamicTableManager\bin\Release\net8.0\win-x64\publish\ETL_DynamicTableManager.exe
echo         UniversalExcelTool.UI\bin\Release\net8.0\win-x64\publish\UniversalExcelTool.UI.exe
echo.
echo [INFO] These applications can now run on any Windows x64 machine
echo        without requiring .NET runtime installation!
echo.
echo [INFO] File sizes are larger due to included runtime, but
echo        deployment is simplified for end users.
echo.

pause
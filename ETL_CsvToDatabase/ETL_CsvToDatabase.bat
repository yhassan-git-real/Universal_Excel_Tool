@echo off
REM ETL_CsvToDatabase Launcher Script
REM This script runs the CSV to Database ETL module

echo ========================================
echo ETL CSV to Database Loader
echo ========================================
echo.

REM Check for published self-contained executable first
if exist "bin\Release\net8.0\win-x64\publish\ETL_CsvToDatabase.exe" (
    echo Running published self-contained executable...
    "bin\Release\net8.0\win-x64\publish\ETL_CsvToDatabase.exe" %*
    goto :end
)

REM Check for self-contained executable (non-published)
if exist "bin\Release\net8.0\win-x64\ETL_CsvToDatabase.exe" (
    echo Running self-contained executable...
    "bin\Release\net8.0\win-x64\ETL_CsvToDatabase.exe" %*
    goto :end
)

REM Fall back to debug build
if exist "bin\Debug\net8.0\ETL_CsvToDatabase.exe" (
    echo Running debug build...
    "bin\Debug\net8.0\ETL_CsvToDatabase.exe" %*
    goto :end
)

REM If no executable found, show instructions
echo ERROR: Executable not found!
echo.
echo Please build the project first:
echo   1. Run: dotnet build
echo   2. Or run: dotnet publish -c Release -r win-x64 --self-contained true
echo.
pause
exit /b 1

:end
echo.
echo Process completed.
pause

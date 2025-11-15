@echo off
REM Build ETL_CsvToDatabase as self-contained executable

echo ========================================
echo Building ETL CSV to Database (Self-Contained)
echo ========================================
echo.

echo Cleaning previous builds...
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj\Release" rmdir /s /q "obj\Release"

echo.
echo Building self-contained executable for Windows x64...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build completed successfully!
    echo ========================================
    echo.
    echo Executable location:
    echo   bin\Release\net8.0\win-x64\ETL_CsvToDatabase.exe
    echo.
    echo File size: ~80-100MB (includes .NET runtime)
    echo.
) else (
    echo.
    echo ========================================
    echo Build failed!
    echo ========================================
    echo.
    echo Please check the error messages above.
    echo.
)

pause

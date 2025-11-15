@echo off
echo ========================================
echo Universal Excel Tool UI
echo ========================================
echo.

REM Kill any running instances
taskkill /F /IM UniversalExcelTool.UI.exe 2>nul

REM Build Core module first (UI references Core DLL)
echo Building Core module...
dotnet build "..\Core\UniversalExcelTool.csproj" -c Release
if errorlevel 1 (
    echo Core build failed!
    pause
    exit /b 1
)

REM Check if Release build exists
if not exist "bin\Release\net8.0\UniversalExcelTool.UI.exe" (
    echo Release build not found. Building UI...
    dotnet build -c Release
    if errorlevel 1 (
        echo Build failed!
        pause
        exit /b 1
    )
    echo Build completed successfully.
    echo.
)

REM Run the application
echo Starting UI Application...
start "" "bin\Release\net8.0\UniversalExcelTool.UI.exe"

echo Application launched successfully.

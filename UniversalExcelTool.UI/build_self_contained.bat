@echo off
REM Build Self-Contained Universal Excel Tool UI
REM This creates a standalone executable with all dependencies

echo ============================================
echo Building Universal Excel Tool UI
echo Self-Contained Release Build
echo ============================================
echo.

REM Clean previous builds
echo [1/5] Cleaning previous builds...
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj\Release" rmdir /s /q "obj\Release"
echo Done.
echo.

REM Build Core project first
echo [2/5] Building Core project...
cd ..\Core
dotnet build -c Release
cd ..\UniversalExcelTool.UI
echo Done.
echo.

REM Restore dependencies
echo [3/5] Restoring NuGet packages...
dotnet restore
echo Done.
echo.

REM Build self-contained for Windows x64
echo [4/5] Building self-contained application...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
echo Done.
echo.

REM Copy configuration files
echo [5/5] Copying configuration files...
copy "..\..\appsettings.json" "bin\Release\net8.0\win-x64\publish\" /Y
copy "..\..\dynamic_table_config.json" "bin\Release\net8.0\win-x64\publish\" /Y
echo Done.
echo.

echo ============================================
echo Build Complete!
echo ============================================
echo.
echo Output location: bin\Release\net8.0\win-x64\publish\
echo Executable: UniversalExcelTool.UI.exe
echo.
echo You can now:
echo 1. Run the executable from the publish folder
echo 2. Create a desktop shortcut to UniversalExcelTool.UI.exe
echo 3. Copy the entire publish folder to another machine
echo.
pause

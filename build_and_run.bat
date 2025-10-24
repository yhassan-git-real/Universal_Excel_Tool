@echo off
echo Building Core project...
dotnet build "f:\Projects-Hub\Universal_Excel_Tool\Core\UniversalExcelTool.csproj"
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo Build successful! Running ETL process...
"f:\Projects-Hub\Universal_Excel_Tool\Core\bin\Debug\net8.0\UniversalExcelTool.exe"
pause
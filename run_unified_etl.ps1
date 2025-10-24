# Universal Excel Tool - Unified ETL Process Launcher
# Uses the centralized orchestrator with unified configuration

param(
    [switch]$ShowConfig,
    [switch]$SkipDynamicConfig,
    [switch]$ContinueOnError,
    [switch]$DynamicTableOnly,
    [switch]$ExcelOnly,
    [switch]$DatabaseOnly,
    [string]$RootDirectory = "",
    [string]$DynamicArgs = "",
    [string]$ExcelArgs = "",
    [string]$DatabaseArgs = "",
    [switch]$Help
)

# Set error handling
$ErrorActionPreference = "Stop"

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Define paths - try self-contained first, then Release, then Debug
$OrchestratorSelfContained = Join-Path $ScriptDir "Core\bin\Release\net8.0\win-x64\publish\UniversalExcelTool.exe"
$OrchestratorRelease = Join-Path $ScriptDir "Core\bin\Release\net8.0\UniversalExcelTool.exe"
$OrchestratorDebug = Join-Path $ScriptDir "Core\bin\Debug\net8.0\UniversalExcelTool.exe"
$AppSettingsPath = Join-Path $ScriptDir "appsettings.json"

if (Test-Path $OrchestratorSelfContained) {
    $OrchestratorPath = $OrchestratorSelfContained
    $BuildType = "Self-Contained"
} elseif (Test-Path $OrchestratorRelease) {
    $OrchestratorPath = $OrchestratorRelease
    $BuildType = "Release"
} elseif (Test-Path $OrchestratorDebug) {
    $OrchestratorPath = $OrchestratorDebug
    $BuildType = "Debug"
} else {
    $OrchestratorPath = $OrchestratorSelfContained
    $BuildType = "Not Found"
}

function Write-Header {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host "         UNIVERSAL EXCEL TOOL - UNIFIED ETL LAUNCHER" -ForegroundColor Green  
    Write-Host "================================================================" -ForegroundColor Green
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Cyan
}

function Test-Prerequisites {
    $success = $true
    
    # Check if appsettings.json exists
    if (!(Test-Path $AppSettingsPath)) {
        Write-Host "✗ Configuration file not found: $AppSettingsPath" -ForegroundColor Red
        Write-Host "ℹ Please ensure appsettings.json is in the root directory" -ForegroundColor Cyan
        $success = $false
    }
    
    # Check if orchestrator executable exists
    if ($BuildType -eq "Not Found") {
        Write-Host "✗ Orchestrator executable not found in any configuration" -ForegroundColor Red
        Write-Host "ℹ Expected locations:" -ForegroundColor Cyan
        Write-Host "ℹ   Self-Contained: $OrchestratorSelfContained" -ForegroundColor Cyan
        Write-Host "ℹ   Release: $OrchestratorRelease" -ForegroundColor Cyan
        Write-Host "ℹ   Debug: $OrchestratorDebug" -ForegroundColor Cyan
        Write-Host "ℹ Please build the project first:" -ForegroundColor Cyan
        Write-Host "ℹ   For self-contained: .\build_self_contained.bat" -ForegroundColor Cyan
        Write-Host "ℹ   For regular build: cd Core; dotnet build --configuration Release" -ForegroundColor Cyan
        $success = $false
    } else {
        Write-Host "ℹ Using $BuildType build: $OrchestratorPath" -ForegroundColor Cyan
    }
    
    return $success
}

function Invoke-UniversalExcelTool {
    param([string[]]$Arguments)
    
    try {
        Write-Host "ℹ Launching Universal Excel Tool Orchestrator..." -ForegroundColor Cyan
        Write-Host "ℹ Command: $OrchestratorPath $($Arguments -join ' ')" -ForegroundColor Cyan
        Write-Host ""
        
        $process = Start-Process -FilePath $OrchestratorPath -ArgumentList $Arguments -Wait -PassThru -NoNewWindow
        
        if ($process.ExitCode -eq 0) {
            Write-Host "✓ Process completed successfully" -ForegroundColor Green
            return $true
        } else {
            Write-Host "✗ Process failed with exit code: $($process.ExitCode)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "✗ Error running orchestrator: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Main execution
try {
    Write-Header
    
    # Display help if requested
    if ($Help) {
        & $OrchestratorPath --help
        Write-Host ""
        Write-Host "PowerShell Usage Examples:" -ForegroundColor Yellow
        Write-Host "  .\run_unified_etl.ps1                           # Run complete ETL process"
        Write-Host "  .\run_unified_etl.ps1 -ShowConfig              # Show current configuration"
        Write-Host "  .\run_unified_etl.ps1 -SkipDynamicConfig       # Skip dynamic table configuration"
        Write-Host "  .\run_unified_etl.ps1 -DynamicTableOnly        # Run only table configuration"
        Write-Host "  .\run_unified_etl.ps1 -RootDirectory 'C:\Path' # Update root directory"
        Write-Host "  .\run_unified_etl.ps1 -Help                    # Show detailed help"
        exit 0
    }
    
    # Check prerequisites
    if (!(Test-Prerequisites)) {
        Write-Host ""
        Write-Host "ℹ Setup Instructions:" -ForegroundColor Cyan
        Write-Host "ℹ 1. Ensure appsettings.json is configured with your environment settings" -ForegroundColor Cyan
        Write-Host "ℹ 2. Build the Core project: cd Core; dotnet build --configuration Release" -ForegroundColor Cyan
        Write-Host "ℹ 3. Update the root directory in appsettings.json if needed" -ForegroundColor Cyan
        exit 1
    }
    
    # Build command line arguments
    $args = @()
    
    if ($ShowConfig) { $args += "--show-config" }
    if ($SkipDynamicConfig) { $args += "--skip-dynamic-config" }
    if ($ContinueOnError) { $args += "--continue-on-error" }
    if ($DynamicTableOnly) { $args += "--dynamic-table-only" }
    if ($ExcelOnly) { $args += "--excel-only" }
    if ($DatabaseOnly) { $args += "--database-only" }
    
    if ($RootDirectory) { 
        $args += "--root-directory"
        $args += $RootDirectory
    }
    
    if ($DynamicArgs) { 
        $args += "--dynamic-args"
        $args += $DynamicArgs
    }
    
    if ($ExcelArgs) { 
        $args += "--excel-args"
        $args += $ExcelArgs
    }
    
    if ($DatabaseArgs) { 
        $args += "--database-args"
        $args += $DatabaseArgs
    }
    
    # Execute the orchestrator
    $success = Invoke-UniversalExcelTool -Arguments $args
    
    Write-Host ""
    if ($success) {
        Write-Host "================================================================" -ForegroundColor Green
        Write-Host "                 PROCESS COMPLETED SUCCESSFULLY" -ForegroundColor Green
        Write-Host "================================================================" -ForegroundColor Green
    } else {
        Write-Host "================================================================" -ForegroundColor Red
        Write-Host "                   PROCESS COMPLETED WITH ERRORS" -ForegroundColor Red
        Write-Host "================================================================" -ForegroundColor Red
    }
    
    if ($success) {
        exit 0
    } else {
        exit 1
    }
}
catch {
    Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "ℹ For help, run: .\run_unified_etl.ps1 -Help" -ForegroundColor Cyan
    exit 1
}
# Universal Excel Tool 🚀

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

**Enterprise-grade ETL system for processing large-scale Excel workbooks and importing data into SQL Server databases.**

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Modules](#modules)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

---

## 🎯 Overview

Universal Excel Tool is a modular ETL (Extract, Transform, Load) system designed to:

- **Extract** data from multi-sheet Excel workbooks
- **Transform** by splitting sheets into individual files and categorizing them
- **Load** data into SQL Server with validation and error handling

The system features a unified orchestration layer, dynamic configuration management, and dual interfaces (Console + Desktop UI).

---

## ✨ Features

### Core Capabilities
- ✅ **Automated ETL Pipeline** - Complete workflow from Excel to database
- ✅ **Dynamic Table Configuration** - Runtime table mapping with user input
- ✅ **Parallel Excel Processing** - Multi-threaded sheet extraction
- ✅ **Smart Sheet Categorization** - Keyword-based routing (SUP, DEM)
- ✅ **Column Validation** - Pre-import schema matching
- ✅ **Bulk Data Import** - Optimized SqlBulkCopy (1M+ rows/batch)
- ✅ **Comprehensive Logging** - Console, file, and database audit trails
- ✅ **Error Recovery** - Continue-on-error support
- ✅ **Modern Desktop UI** - Avalonia-based interface (Phase 1+2 complete)

### Technical Highlights
- 🔧 **Self-Contained Deployment** - No external runtime required
- 🌍 **Location-Agnostic** - Auto-detects root directory
- 🔄 **Modular Architecture** - Independent, reusable components
- 📊 **Performance Optimized** - Handles millions of rows efficiently
- 🛡️ **Production-Ready** - Transaction safety, rollback support

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│              ORCHESTRATION LAYER                        │
│  Core/UniversalExcelTool.exe (ETL Orchestrator)        │
└─────────────────────────────────────────────────────────┘
                          ▼
    ┌─────────────────────────────────────────┐
    │    DYNAMIC CONFIGURATION                │
    │  (dynamic_table_config.json)            │
    └─────────────────────────────────────────┘
                          ▼
┌──────────────┬────────────────────┬─────────────────┐
│  MODULE 1    │     MODULE 2       │    MODULE 3     │
│  Dynamic     │  Excel Processor   │ Database Loader │
│  Table Mgr   │                    │                 │
└──────────────┴────────────────────┴─────────────────┘
                          ▼
            ┌──────────────────────┐
            │   SQL SERVER DB      │
            └──────────────────────┘
```

**Workflow:**
1. **Configure** - Set table mappings interactively
2. **Process** - Split Excel sheets into individual files
3. **Import** - Load data with validation into SQL Server
4. **Audit** - Log all operations for traceability

---

## 📦 Prerequisites

### Required
- **Operating System:** Windows 10/11 or Windows Server 2016+
- **.NET SDK:** .NET 8.0 SDK (for building from source)
- **Database:** SQL Server 2016+ or Azure SQL Database
- **Memory:** 4GB RAM minimum (8GB+ recommended for large files)
- **Disk Space:** 500MB for application + space for Excel files

### Optional
- **Git:** For version control
- **Visual Studio 2022** or **VS Code** (for development)

---

## 🚀 Installation

### Option 1: Download Release Build
```bash
# Download the latest release from GitHub
# Extract to your desired location
# Update appsettings.json with your paths
```

### Option 2: Build from Source

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/Universal_Excel_Tool.git
   cd Universal_Excel_Tool
   ```

2. **Build all projects:**
   ```bash
   # Self-contained build (includes .NET runtime)
   .\build_self_contained.bat
   
   # Or build and run immediately
   .\build_and_run.bat
   ```

3. **Configure the application:**
   - Edit `appsettings.json` with your database and path settings
   - See [Configuration](#configuration) section below

---

## ⚙️ Configuration

### `appsettings.json` (Main Configuration)

```json
{
  "Environment": {
    "RootDirectory": "F:\\Projects-Hub\\Universal_Excel_Tool",
    "Environment": "Production"
  },
  "Database": {
    "Server": "YOUR_SERVER\\INSTANCE",
    "Database": "RAW_PROCESS",
    "IntegratedSecurity": true
  },
  "Paths": {
    "RawExcelFiles": "E:\\Files",
    "ExcelFiles": "E:\\Files\\ExcelFiles",
    "ProcessedFiles": "E:\\Files\\Special_Sheets_Excels",
    "Logs": "F:\\Projects-Hub\\Universal_Excel_Tool\\Logs"
  },
  "Processing": {
    "BatchSize": 1000000,
    "ValidateColumnMapping": true,
    "SpecialSheetKeywords": ["SUP", "DEM"]
  }
}
```

**Key Settings:**
- `RootDirectory`: Project root (auto-detected if empty)
- `Database.Server`: SQL Server instance name
- `Paths.RawExcelFiles`: Input directory for Excel files
- `SpecialSheetKeywords`: Keywords for special sheet routing

---

## 💻 Usage

### Complete ETL Process
```bash
# Run the orchestrator (all modules)
.\Core\bin\Release\net8.0\win-x64\UniversalExcelTool.exe
```

### Individual Modules

**1. Configure Table Mappings:**
```bash
.\ETL_DynamicTableManager\bin\Release\net8.0\win-x64\ETL_DynamicTableManager.exe
```

**2. Process Excel Files:**
```bash
.\ETL_Excel\bin\Release\net8.0\win-x64\ETL_Excel.exe --non-interactive
```

**3. Import to Database:**
```bash
.\ETL_ExcelToDatabase\bin\Release\net8.0\win-x64\ETL_ExcelToDatabase.exe
```

### Command Line Options

```bash
# Show help
UniversalExcelTool.exe --help

# Skip dynamic configuration (use existing)
UniversalExcelTool.exe --skip-dynamic-config

# Continue on errors
UniversalExcelTool.exe --continue-on-error

# Run specific module only
UniversalExcelTool.exe --excel-only
UniversalExcelTool.exe --database-only

# Update root directory
UniversalExcelTool.exe --root-directory "C:\NewPath"
```

### Desktop UI (Phase 1+2 Complete)

```bash
# Launch Avalonia UI
.\UniversalExcelTool.UI\bin\Release\net8.0\win-x64\UniversalExcelTool.UI.exe
```

---

## 📂 Project Structure

```
Universal_Excel_Tool/
├── Core/                           # Orchestrator & configuration
│   ├── Program.cs                  # Entry point
│   ├── ETLOrchestrator.cs          # Module coordinator
│   └── UnifiedConfigurationManager.cs
│
├── ETL_DynamicTableManager/        # Interactive table config
│   ├── Program.cs
│   ├── Services/
│   │   ├── TableConfigurationService.cs
│   │   └── UserInputService.cs
│   └── Core/DatabaseOperations.cs
│
├── ETL_Excel/                      # Excel processor
│   ├── Program.cs
│   └── Modules/
│       ├── ExcelProcessor.cs       # Sheet splitting logic
│       ├── FileManager.cs
│       └── ConfigurationManager.cs
│
├── ETL_ExcelToDatabase/            # Database loader
│   ├── Program.cs
│   ├── Core/DatabaseOperations.cs  # Bulk import
│   └── Services/
│       ├── ValidationService.cs
│       └── LoggingService.cs
│
├── UniversalExcelTool.UI/          # Avalonia desktop app
│   ├── ViewModels/
│   ├── Views/
│   └── Services/
│
├── appsettings.json                # Main configuration
├── dynamic_table_config.json       # Runtime config
├── .gitignore
└── README.md
```

---

## 🔧 Modules

### 1. ETL Orchestrator (`Core/`)
**Purpose:** Centralized command center for ETL pipeline

- Loads unified configuration
- Executes modules sequentially
- Handles errors and logging
- Provides CLI interface

### 2. Dynamic Table Manager (`ETL_DynamicTableManager/`)
**Purpose:** Interactive table configuration wizard

- Prompts for temp/destination table names
- Validates database connectivity
- Checks table existence
- Saves configuration for other modules

### 3. Excel Processor (`ETL_Excel/`)
**Purpose:** Split multi-sheet workbooks

- Parallel sheet processing
- Keyword-based categorization (SUP, DEM)
- Formula and formatting preservation
- Output: Individual Excel files per sheet

### 4. Database Loader (`ETL_ExcelToDatabase/`)
**Purpose:** Bulk import with validation

- Three-phase import (Staging → Validation → Transfer)
- Column mapping validation
- SqlBulkCopy optimization
- Comprehensive error logging

### 5. Desktop UI (`UniversalExcelTool.UI/`)
**Purpose:** Modern visual interface

- MVVM architecture (Avalonia)
- Dashboard with status cards
- Real-time log viewer
- Configuration wizard

---

## 🛠️ Development

### Building the Solution

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Self-contained (includes runtime)
dotnet publish -c Release -r win-x64 --self-contained true
```

### Running Tests

```bash
# Run all tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

### Adding New Modules

1. Create new .NET 8.0 console/library project
2. Reference `Core/UnifiedConfigurationManager.cs`
3. Add module info to `appsettings.json`:
   ```json
   "ExecutableModules": {
     "YourModule": {
       "RelativePath": "YourModule\\bin\\Release\\...",
       "Name": "Your Module Name",
       "Order": 4
     }
   }
   ```

---

## 🐛 Troubleshooting

### Common Issues

**1. Database Connection Failed**
- Verify SQL Server is running
- Check `appsettings.json` server name
- Ensure Windows Authentication or correct credentials
- Test with: `UniversalExcelTool.exe --show-config`

**2. Excel Files Not Found**
- Check `Paths.RawExcelFiles` in configuration
- Ensure directory exists
- Verify file extensions (.xlsx, .xls)

**3. Column Validation Errors**
- Check `Error_table` for details
- Ensure Excel headers match database columns
- Review validation report in logs

**4. Module Not Found**
- Build solution first: `.\build_self_contained.bat`
- Check executable paths in `appsettings.json`
- Try Debug build path if Release doesn't exist

### Logs Location
- **Orchestrator Logs:** `Logs/ETL_Orchestrator_YYYYMMDD_HHMMSS.txt`
- **Console Output:** `Logs/Console_output_log_YYYYMMDD_HHMMSS.txt`
- **Database Errors:** `Error_table` in SQL Server
- **Success Audit:** `Success_table` in SQL Server

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards
- Follow C# naming conventions
- Add XML documentation for public APIs
- Include unit tests for new features
- Update README for significant changes

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📞 Support

For issues, questions, or suggestions:
- **GitHub Issues:** [Create an issue](https://github.com/yourusername/Universal_Excel_Tool/issues)
- **Documentation:** See [DEPLOYMENT_README.md](DEPLOYMENT_README.md)

---

## 🎉 Acknowledgments

- Built with **.NET 8.0**
- Excel processing powered by **ClosedXML**
- UI framework: **Avalonia UI**
- Database: **Microsoft SQL Server**

---

**Made with ❤️ for efficient data processing**

*Last Updated: October 24, 2025*

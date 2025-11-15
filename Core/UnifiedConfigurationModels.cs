using System.Collections.Generic;

namespace UniversalExcelTool.Core
{
    /// <summary>
    /// Unified configuration model for the entire Universal Excel Tool system
    /// </summary>
    public class UnifiedConfig
    {
        public EnvironmentConfig Environment { get; set; } = new();
        public DatabaseConfig Database { get; set; } = new();
        public PathsConfig Paths { get; set; } = new();
        public ExecutableModulesConfig ExecutableModules { get; set; } = new();
        public ProcessingConfig Processing { get; set; } = new();
        public LoggingConfig Logging { get; set; } = new();
        public TablesConfig Tables { get; set; } = new();
    }

    /// <summary>
    /// Environment-specific configuration
    /// </summary>
    public class EnvironmentConfig
    {
        public string RootDirectory { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Database configuration
    /// </summary>
    public class DatabaseConfig
    {
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public bool IntegratedSecurity { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int ConnectionTimeout { get; set; } = 0;
        public int CommandTimeout { get; set; } = 0;
    }

    /// <summary>
    /// File and directory paths configuration (all relative to root)
    /// </summary>
    public class PathsConfig
    {
        public string InputExcelFiles { get; set; } = string.Empty;      // Input: Raw Excel files to process
        public string InputCsvFiles { get; set; } = string.Empty;        // Input: Raw CSV files to process
        public string OutputExcelFiles { get; set; } = string.Empty;     // Output: Regular processed Excel files
        public string SpecialExcelFiles { get; set; } = string.Empty;    // Output: Special categorized Excel files (SUP, DEM)
        public string LogFiles { get; set; } = string.Empty;             // Logs: Application logs and audit trails
        public string TempFiles { get; set; } = string.Empty;            // Temp: Temporary processing files
    }

    /// <summary>
    /// Executable modules configuration
    /// </summary>
    public class ExecutableModulesConfig
    {
        public ExecutableModuleInfo DynamicTableManager { get; set; } = new();
        public ExecutableModuleInfo ExcelProcessor { get; set; } = new();
        public ExecutableModuleInfo DatabaseLoader { get; set; } = new();
        public ExecutableModuleInfo CsvToDatabase { get; set; } = new();
    }

    /// <summary>
    /// Individual executable module information
    /// </summary>
    public class ExecutableModuleInfo
    {
        public string RelativePath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public string Arguments { get; set; } = string.Empty;
    }

    /// <summary>
    /// Processing configuration
    /// </summary>
    public class ProcessingConfig
    {
        public int BatchSize { get; set; } = 0;
        public bool ValidateColumnMapping { get; set; } = false;
        public string DefaultSheetName { get; set; } = string.Empty;
        public int MaxConcurrentFiles { get; set; } = 0;
        public int RetryAttempts { get; set; } = 0;
        public int RetryDelaySeconds { get; set; } = 0;
        public List<string> SpecialSheetKeywords { get; set; } = new();
    }

    /// <summary>
    /// Logging configuration
    /// </summary>
    public class LoggingConfig
    {
        public string Level { get; set; } = string.Empty;
        public bool EnableFileLogging { get; set; } = false;
        public bool EnableConsoleLogging { get; set; } = false;
        public int LogRetentionDays { get; set; } = 0;
        public int MaxLogFileSizeMB { get; set; } = 0;
    }

    /// <summary>
    /// Tables configuration (predefined tables)
    /// </summary>
    public class TablesConfig
    {
        public string ErrorTableName { get; set; } = string.Empty;
        public string SuccessLogTableName { get; set; } = string.Empty;
        public bool AutoCreateLogTables { get; set; } = true;
    }

    /// <summary>
    /// Dynamic table configuration (runtime configurable)
    /// </summary>
    public class DynamicTableConfig
    {
        public string TempTableName { get; set; } = string.Empty;
        public string DestinationTableName { get; set; } = string.Empty;
        public bool TargetTableExists { get; set; }
        public bool ShouldTruncateTable { get; set; }
        public bool CreateNewTable { get; set; }
        public DateTime ConfigurationTimestamp { get; set; } = DateTime.Now;
    }
}
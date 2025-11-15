namespace ETL_DynamicTableManager.Models
{
    /// <summary>
    /// Configuration model for dynamic table management
    /// </summary>
    public class DynamicTableConfig
    {
        public string TempTableName { get; set; } = string.Empty;
        public string DestinationTableName { get; set; } = string.Empty;
        public string ErrorTableName { get; set; } = string.Empty;
        public string SuccessLogTableName { get; set; } = string.Empty;
        public bool TargetTableExists { get; set; }
        public bool ShouldTruncateTable { get; set; }
        public bool CreateNewTable { get; set; }
        public DateTime ConfigurationTimestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Database configuration model
    /// </summary>
    public class DatabaseConfig
    {
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public bool IntegratedSecurity { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int ConnectionTimeout { get; set; } = 600;
    }

    /// <summary>
    /// Main application configuration
    /// </summary>
    public class AppConfig
    {
        public DatabaseConfig DatabaseConfig { get; set; } = new();
        public ProcessConfig ProcessConfig { get; set; } = new();
    }

    /// <summary>
    /// Process configuration
    /// </summary>
    public class ProcessConfig
    {
        public string ExcelFolderPath { get; set; } = string.Empty;
        public string LogFolderPath { get; set; } = string.Empty;
        public string TempTableName { get; set; } = string.Empty;
        public string DestinationTableName { get; set; } = string.Empty;
        public string ErrorTableName { get; set; } = string.Empty;
        public string SuccessLogTableName { get; set; } = string.Empty;
        public int BatchSize { get; set; } = 1000000;
        public bool ValidateColumnMapping { get; set; } = true;
        public string DefaultSheetName { get; set; } = "null";
    }

    /// <summary>
    /// ETL Configuration container
    /// </summary>
    public class EtlConfig
    {
        public DatabaseConfig DatabaseConfig { get; set; } = new();
        public ProcessConfig ProcessConfig { get; set; } = new();
    }

    /// <summary>
    /// Result of table existence check
    /// </summary>
    public class TableExistenceResult
    {
        public bool Exists { get; set; }
        public string TableName { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// User input result
    /// </summary>
    public class UserInputResult
    {
        public bool IsValid { get; set; }
        public string Value { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
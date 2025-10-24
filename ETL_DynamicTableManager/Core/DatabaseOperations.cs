using Microsoft.Data.SqlClient;
using ETL_DynamicTableManager.Models;
using System.Data;

namespace ETL_DynamicTableManager.Core
{
    /// <summary>
    /// Database operations for dynamic table management
    /// </summary>
    public static class DatabaseOperations
    {
        /// <summary>
        /// Tests database connection
        /// </summary>
        public static async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("ℹ Testing database connection...");
                Console.ResetColor();
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Database connection successful");
                Console.ResetColor();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Database connection failed: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        /// Checks if a table exists in the database
        /// </summary>
        public static async Task<TableExistenceResult> CheckTableExistsAsync(string connectionString, string tableName)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"ℹ Checking if table '{tableName}' exists...");
                Console.ResetColor();
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                string query = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = @TableName 
                    AND TABLE_TYPE = 'BASE TABLE'";
                
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                
                var result = await command.ExecuteScalarAsync();
                bool exists = Convert.ToInt32(result) > 0;
                
                var tableResult = new TableExistenceResult
                {
                    Exists = exists,
                    TableName = tableName
                };
                
                if (exists)
                {
                    // Get column information
                    tableResult.Columns = await GetTableColumnsAsync(connection, tableName);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Table '{tableName}' exists with {tableResult.Columns.Count} columns");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Table '{tableName}' does not exist");
                    Console.ResetColor();
                }
                
                return tableResult;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error checking table existence: {ex.Message}");
                Console.ResetColor();
                
                return new TableExistenceResult
                {
                    Exists = false,
                    TableName = tableName,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets the column names of a table
        /// </summary>
        public static async Task<List<string>> GetTableColumnsAsync(SqlConnection connection, string tableName)
        {
            try
            {
                string query = @"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @TableName 
                    ORDER BY ORDINAL_POSITION";
                
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                
                var columns = new List<string>();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString("COLUMN_NAME"));
                }
                
                return columns;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error getting table columns: {ex.Message}");
                Console.ResetColor();
                return new List<string>();
            }
        }

        /// <summary>
        /// Creates a new table based on temp table structure
        /// </summary>
        public static async Task<bool> CreateTableFromTempAsync(string connectionString, string tempTableName, string newTableName)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"ℹ Creating new table '{newTableName}' based on '{tempTableName}' structure...");
                Console.ResetColor();
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                // First check if temp table exists
                var tempExists = await CheckTableExistsAsync(connectionString, tempTableName);
                if (!tempExists.Exists)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Cannot create table: Temporary table '{tempTableName}' does not exist");
                    Console.ResetColor();
                    return false;
                }
                
                // Get the structure of the temp table
                string getStructureQuery = @"
                    SELECT 
                        COLUMN_NAME,
                        DATA_TYPE,
                        CHARACTER_MAXIMUM_LENGTH,
                        IS_NULLABLE
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @TempTableName 
                    ORDER BY ORDINAL_POSITION";
                
                using var structureCommand = new SqlCommand(getStructureQuery, connection);
                structureCommand.Parameters.AddWithValue("@TempTableName", tempTableName);
                
                var columns = new List<string>();
                using var reader = await structureCommand.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    string columnName = reader.GetString("COLUMN_NAME");
                    string dataType = reader.GetString("DATA_TYPE");
                    var maxLength = reader.IsDBNull("CHARACTER_MAXIMUM_LENGTH") ? (int?)null : reader.GetInt32("CHARACTER_MAXIMUM_LENGTH");
                    string isNullable = reader.GetString("IS_NULLABLE");
                    
                    string columnDef = $"[{columnName}] {dataType}";
                    
                    if (maxLength.HasValue && maxLength.Value > 0)
                    {
                        columnDef += $"({maxLength.Value})";
                    }
                    else if (dataType.ToUpper() == "NVARCHAR")
                    {
                        columnDef += "(MAX)";
                    }
                    
                    if (isNullable == "NO")
                    {
                        columnDef += " NOT NULL";
                    }
                    
                    columns.Add(columnDef);
                }
                
                reader.Close();
                
                if (!columns.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ No columns found in temporary table '{tempTableName}'");
                    Console.ResetColor();
                    return false;
                }
                
                // Create the new table
                string createTableQuery = $@"
                    CREATE TABLE [{newTableName}] (
                        {string.Join(",\n        ", columns)}
                    )";
                
                using var createCommand = new SqlCommand(createTableQuery, connection);
                await createCommand.ExecuteNonQueryAsync();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Table '{newTableName}' created successfully with {columns.Count} columns");
                Console.ResetColor();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error creating table: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        /// Truncates a table (removes all data)
        /// </summary>
        public static async Task<bool> TruncateTableAsync(string connectionString, string tableName)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"ℹ Truncating table '{tableName}'...");
                Console.ResetColor();
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                string truncateQuery = $"TRUNCATE TABLE [{tableName}]";
                
                using var command = new SqlCommand(truncateQuery, connection);
                await command.ExecuteNonQueryAsync();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Table '{tableName}' truncated successfully");
                Console.ResetColor();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error truncating table: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        /// Gets table row count
        /// </summary>
        public static async Task<long> GetTableRowCountAsync(string connectionString, string tableName)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                string query = $"SELECT COUNT(*) FROM [{tableName}]";
                using var command = new SqlCommand(query, connection);
                
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error getting row count for table '{tableName}': {ex.Message}");
                Console.ResetColor();
                return -1;
            }
        }

        /// <summary>
        /// Validates that all required tables can be accessed
        /// </summary>
        public static async Task<bool> ValidateTableAccessAsync(string connectionString, DynamicTableConfig config)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("ℹ Validating table access...");
                Console.ResetColor();
                
                bool allValid = true;
                
                // Check if destination table exists (if it should)
                if (config.TargetTableExists)
                {
                    var destResult = await CheckTableExistsAsync(connectionString, config.DestinationTableName);
                    if (!destResult.Exists)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"✗ Destination table '{config.DestinationTableName}' not found");
                        Console.ResetColor();
                        allValid = false;
                    }
                }
                
                // Note: Log tables are NOT created here - they are handled by the main ETL application
                // Log tables use predefined names from config.json and are created during ETL process
                
                if (allValid)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Table access validation successful");
                    Console.ResetColor();
                }
                
                return allValid;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error validating table access: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
    }
}
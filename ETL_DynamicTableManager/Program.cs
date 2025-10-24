using ETL_DynamicTableManager.Configuration;
using ETL_DynamicTableManager.Core;
using ETL_DynamicTableManager.Models;
using ETL_DynamicTableManager.Services;

namespace ETL_DynamicTableManager
{
    /// <summary>
    /// Main program for dynamic table management
    /// </summary>
    internal static class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                // Display header
                UserInputService.DisplayHeader();
                
                // Parse command line arguments
                bool validateOnly = args.Contains("--validate-only") || args.Contains("-v");
                bool skipUserInput = args.Contains("--skip-input") || args.Contains("-s") || validateOnly;
                bool showCurrentConfig = args.Contains("--show-config") || args.Contains("-c");
                bool deleteConfig = args.Contains("--delete-config") || args.Contains("-d");
                
                // Handle special commands
                if (showCurrentConfig)
                {
                    await TableConfigurationService.DisplayCurrentConfigurationAsync();
                    return 0;
                }
                
                if (deleteConfig)
                {
                    var deleteResult = UserInputService.GetYesNoInput("Are you sure you want to delete the current configuration?");
                    if (deleteResult.Value == "Yes")
                    {
                        TableConfigurationService.DeleteConfiguration();
                    }
                    return 0;
                }
                
                // Load ETL configuration
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("ℹ Loading ETL system configuration...");
                Console.ResetColor();
                
                var etlConfig = ConfigurationLoader.LoadEtlConfiguration();
                
                if (!ConfigurationLoader.ValidateEtlConfiguration(etlConfig))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✗ ETL configuration is invalid. Please check the configuration file.");
                    Console.ResetColor();
                    return 1;
                }
                
                // Ensure directories exist
                ConfigurationLoader.EnsureDirectoriesExist(etlConfig.ProcessConfig);
                
                // Build connection string and test database
                string connectionString = ConfigurationLoader.BuildConnectionString(etlConfig.DatabaseConfig);
                
                if (!await DatabaseOperations.TestConnectionAsync(connectionString))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✗ Cannot proceed without database connection.");
                    Console.ResetColor();
                    return 1;
                }
                
                ConfigurationLoader.DisplayConfigurationSummary(etlConfig);
                
                // Load or create dynamic table configuration
                DynamicTableConfig? dynamicConfig = null;
                
                if (!skipUserInput)
                {
                    dynamicConfig = await GetDynamicTableConfigurationAsync(connectionString);
                    
                    if (dynamicConfig == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("✗ Configuration process was cancelled or failed.");
                        Console.ResetColor();
                        return 1;
                    }
                }
                else
                {
                    // Try to load existing configuration
                    dynamicConfig = await TableConfigurationService.LoadConfigurationAsync();
                    
                    if (dynamicConfig == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("✗ No existing configuration found. Run without --skip-input to create one.");
                        Console.ResetColor();
                        return 1;
                    }
                }
                
                // Validate configuration
                if (!TableConfigurationService.ValidateConfiguration(dynamicConfig))
                {
                    return 1;
                }
                
                if (validateOnly)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Configuration validation successful");
                    Console.ResetColor();
                    return 0;
                }
                
                // Validate database access with the configuration
                if (!await DatabaseOperations.ValidateTableAccessAsync(connectionString, dynamicConfig))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✗ Database validation failed. Please check your table configuration.");
                    Console.ResetColor();
                    return 1;
                }
                
                // Save the final configuration
                if (!await TableConfigurationService.SaveConfigurationAsync(dynamicConfig))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✗ Failed to save configuration.");
                    Console.ResetColor();
                    return 1;
                }
                
                // Display completion message
                UserInputService.DisplayCompletionMessage(TableConfigurationService.GetConfigFilePath());
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓ Dynamic Table Manager completed successfully!");
                Console.WriteLine("The ETL process can now use the dynamically configured table names.");
                Console.ResetColor();
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ An unexpected error occurred: {ex.Message}");
                Console.WriteLine("\nStack trace:");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return 1;
            }
        }

        /// <summary>
        /// Gets dynamic table configuration from user input
        /// </summary>
        private static async Task<DynamicTableConfig?> GetDynamicTableConfigurationAsync(string connectionString)
        {
            try
            {
                // Check if configuration already exists
                var existingConfig = await TableConfigurationService.LoadConfigurationAsync();
                
                if (existingConfig != null)
                {
                    var useExistingResult = UserInputService.GetYesNoInput("A configuration already exists. Do you want to use it?");
                    
                    if (useExistingResult.Value == "Yes")
                    {
                        await TableConfigurationService.DisplayCurrentConfigurationAsync();
                        var proceedResult = UserInputService.GetYesNoInput("Do you want to proceed with this existing configuration?");
                        
                        if (proceedResult.Value == "Yes")
                        {
                            return existingConfig;
                        }
                    }
                    
                    Console.WriteLine("Creating new configuration...");
                }
                
                // Create new configuration
                var config = TableConfigurationService.CreateDefaultConfig();
                
                // Step 1: Get temporary table name
                var tempTableResult = UserInputService.GetTempTableNameInput();
                if (!tempTableResult.IsValid)
                {
                    return null;
                }
                config.TempTableName = tempTableResult.Value;
                
                // Step 2: Ask if target table exists
                var tableExistsResult = UserInputService.AskIfTargetTableExists();
                if (!tableExistsResult.IsValid)
                {
                    return null;
                }
                
                config.TargetTableExists = tableExistsResult.Value == "Yes";
                
                if (config.TargetTableExists)
                {
                    // Step 3a: Get existing table name
                    var existingTableResult = UserInputService.GetExistingTableName();
                    if (!existingTableResult.IsValid)
                    {
                        return null;
                    }
                    config.DestinationTableName = existingTableResult.Value;
                    
                    // Verify table exists in database
                    var tableCheck = await DatabaseOperations.CheckTableExistsAsync(connectionString, config.DestinationTableName);
                    if (!tableCheck.Exists)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"✗ Table '{config.DestinationTableName}' was not found in the database.");
                        Console.ResetColor();
                        
                        var retryResult = UserInputService.GetYesNoInput("Do you want to enter a different table name?");
                        if (retryResult.Value == "Yes")
                        {
                            return await GetDynamicTableConfigurationAsync(connectionString);
                        }
                        return null;
                    }
                    
                    // Step 4a: Ask about truncation
                    var truncateResult = UserInputService.AskToTruncateTable(config.DestinationTableName);
                    if (!truncateResult.IsValid)
                    {
                        return null;
                    }
                    config.ShouldTruncateTable = truncateResult.Value == "Yes";
                }
                else
                {
                    // Step 3b: Ask to create new table
                    var createTableResult = UserInputService.AskToCreateNewTable();
                    if (!createTableResult.IsValid)
                    {
                        return null;
                    }
                    
                    if (createTableResult.Value == "No")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("✗ Cannot proceed without a target table. Either use an existing table or create a new one.");
                        Console.ResetColor();
                        return null;
                    }
                    
                    config.CreateNewTable = true;
                    
                    // Step 4b: Get new table name
                    var newTableResult = UserInputService.GetNewTableName();
                    if (!newTableResult.IsValid)
                    {
                        return null;
                    }
                    config.DestinationTableName = newTableResult.Value;
                    
                    // Verify table doesn't already exist
                    var tableCheck = await DatabaseOperations.CheckTableExistsAsync(connectionString, config.DestinationTableName);
                    if (tableCheck.Exists)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠ Table '{config.DestinationTableName}' already exists in the database.");
                        Console.ResetColor();
                        
                        var overwriteResult = UserInputService.GetYesNoInput("Do you want to use this existing table instead?");
                        if (overwriteResult.Value == "Yes")
                        {
                            config.TargetTableExists = true;
                            config.CreateNewTable = false;
                            
                            var truncateExistingResult = UserInputService.AskToTruncateTable(config.DestinationTableName);
                            config.ShouldTruncateTable = truncateExistingResult.Value == "Yes";
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("✗ Please choose a different table name that doesn't exist.");
                            Console.ResetColor();
                            return await GetDynamicTableConfigurationAsync(connectionString);
                        }
                    }
                }
                
                // Note: ErrorTableName and SuccessLogTableName are NOT set here
                // They remain predefined in the main ETL application's config.json
                // Only TempTableName and DestinationTableName are dynamic
                
                // Step 5: Confirm configuration
                var confirmResult = UserInputService.ConfirmConfiguration(config);
                if (!confirmResult.IsValid || confirmResult.Value == "No")
                {
                    var reconfigureResult = UserInputService.GetYesNoInput("Do you want to reconfigure?");
                    if (reconfigureResult.Value == "Yes")
                    {
                        return await GetDynamicTableConfigurationAsync(connectionString);
                    }
                    return null;
                }
                
                return config;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error during configuration: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }
    }
}
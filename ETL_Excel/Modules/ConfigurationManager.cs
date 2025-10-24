using ETL_Excel.Models;
using ETL_Excel.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ETL_Excel.Modules
{
    public static class ConfigurationManager
    {
        private static ConfigurationModel? _config;
        private static UnifiedConfigurationManager? _unifiedConfig;

        public static ConfigurationModel GetConfig()
        {
            if (_config == null)
            {
                LoadConfig();
            }
            return _config ?? throw new InvalidOperationException("Configuration could not be loaded");
        }

        private static void LoadConfig()
        {
            try
            {
                // Load from unified configuration
                _unifiedConfig = UnifiedConfigurationManager.Instance;
                var unifiedConfig = _unifiedConfig.GetConfiguration();
                
                // Convert unified config to application format
                _config = new ConfigurationModel
                {
                    InputSettings = new Models.InputSettings
                    {
                        InputFolderPath = _unifiedConfig.GetRawExcelFilesPath() // Base folder for raw Excel files
                    },
                    OutputSettings = new Models.OutputSettings
                    {
                        BaseFolderPath = _unifiedConfig.GetExcelFilesPath(), // Regular processed sheets go to ExcelFiles
                        OtherCategoryFolder = _unifiedConfig.GetProcessedFilesPath(), // Special sheets absolute path: Files\Special_Sheets_Excels
                        SpecialSheetKeywords = unifiedConfig.Processing.SpecialSheetKeywords?.ToList() ?? new List<string>()
                    },
                    LogSettings = new Models.LogSettings
                    {
                        LogFolderPath = _unifiedConfig.GetLogsPath()
                    },
                    ProcessingSettings = new Models.ProcessingSettings
                    {
                        ChunkSize = 10000,
                        SaveInterval = 50000,
                        MemoryCleanupInterval = 5,
                        MaxDegreeOfParallelism = 1,
                        BatchSize = unifiedConfig.Processing.BatchSize
                    }
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load unified configuration: {ex.Message}", ex);
            }
        }
    }
}
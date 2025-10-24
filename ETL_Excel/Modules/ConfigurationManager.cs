﻿using ETL_Excel.Models;
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
                        InputFolderPath = _unifiedConfig.GetInputExcelFilesPath() // Input: Raw Excel files to process
                    },
                    OutputSettings = new Models.OutputSettings
                    {
                        BaseFolderPath = _unifiedConfig.GetOutputExcelFilesPath(), // Output: Regular processed sheets
                        OtherCategoryFolder = _unifiedConfig.GetSpecialExcelFilesPath(), // Output: Special sheets (SUP, DEM)
                        SpecialSheetKeywords = unifiedConfig.Processing.SpecialSheetKeywords?.ToList() ?? new List<string>()
                    },
                    LogSettings = new Models.LogSettings
                    {
                        LogFolderPath = _unifiedConfig.GetLogFilesPath()
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
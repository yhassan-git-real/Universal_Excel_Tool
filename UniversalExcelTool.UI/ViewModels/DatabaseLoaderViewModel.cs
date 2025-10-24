using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniversalExcelTool.UI.Models;
using UniversalExcelTool.UI.Services;
using UniversalExcelTool.Core;

namespace UniversalExcelTool.UI.ViewModels
{
    /// <summary>
    /// View model for database loading operations
    /// </summary>
    public partial class DatabaseLoaderViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<LogEntry> _logEntries = new();

        [ObservableProperty]
        private ProgressInfo _progressInfo = new();

        private readonly IUILogger _logger;
        private readonly IProgressReporter _progressReporter;
        private readonly UnifiedConfigurationManager _configManager;

        public DatabaseLoaderViewModel()
        {
            _logger = new AvaloniaLogger(_logEntries);
            _progressReporter = new AvaloniaProgressReporter(_progressInfo, OnProgressChanged);
            _configManager = UnifiedConfigurationManager.Instance;
            
            _logger.LogInfo("Database Loader view opened", "loader");
        }

        [RelayCommand]
        private async Task RunDatabaseLoaderAsync()
        {
            if (IsBusy)
            {
                _logger.LogWarning("Database Loader is already running");
                return;
            }

            try
            {
                IsBusy = true;
                BusyMessage = "Loading data to database...";
                _progressReporter.Reset();

                var stopwatch = Stopwatch.StartNew();
                _logger.LogInfo("═══════════════════════════════════════════", "loader");
                _logger.LogInfo("Starting Database Loader", "loader");
                _logger.LogInfo("═══════════════════════════════════════════", "loader");

                var orchestrator = new ETLOrchestratorWithLogger(_logger, _progressReporter);
                var options = new ETLProcessOptions
                {
                    SkipDynamicTableConfig = true
                };

                bool success = await Task.Run(() => orchestrator.RunDatabaseLoaderAsync(options));

                stopwatch.Stop();

                if (success)
                {
                    _logger.LogSuccess($"Database loading completed in {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _progressReporter.ReportComplete(true, stopwatch.Elapsed, "Data loaded to database successfully");
                }
                else
                {
                    _logger.LogError($"Database loading failed after {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _progressReporter.ReportComplete(false, stopwatch.Elapsed, "Loading completed with errors");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Database loading error: {ex.Message}");
                _progressReporter.ReportError(ex.Message);
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }

        private void OnProgressChanged()
        {
            // Trigger property change notifications
            OnPropertyChanged(nameof(ProgressInfo));
        }
    }
}

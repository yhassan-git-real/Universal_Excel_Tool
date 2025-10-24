using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniversalExcelTool.UI.Models;
using UniversalExcelTool.UI.Services;
using UniversalExcelTool.Core;
using Microsoft.Data.SqlClient;

namespace UniversalExcelTool.UI.ViewModels
{
    /// <summary>
    /// Dashboard view model showing system overview and quick actions
    /// </summary>
    public partial class DashboardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _databaseStatus = "Checking...";

        [ObservableProperty]
        private bool _isDatabaseConnected;

        [ObservableProperty]
        private string _lastExecutionInfo = "No executions yet";

        [ObservableProperty]
        private int _totalFilesProcessed;

        [ObservableProperty]
        private ObservableCollection<ExecutionStatus> _recentExecutions = new();

        [ObservableProperty]
        private ObservableCollection<LogEntry> _logEntries = new();

        [ObservableProperty]
        private ProgressInfo _progressInfo = new();

        private readonly IUILogger _logger;
        private readonly IProgressReporter _progressReporter;
        private readonly UnifiedConfigurationManager _configManager;

        public DashboardViewModel()
        {
            _logger = new AvaloniaLogger(_logEntries);
            _progressReporter = new AvaloniaProgressReporter(_progressInfo, () => { });
            _configManager = UnifiedConfigurationManager.Instance;
            
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            _logger.LogInfo("Dashboard initialized", "system");
            await CheckDatabaseConnectionAsync();
            LoadRecentExecutions();
        }

        [RelayCommand]
        private async Task CheckDatabaseConnectionAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Checking database connection...";
                _logger.LogInfo("Checking database connection...", "database");

                await Task.Run(() =>
                {
                    string connectionString = _configManager.GetConnectionString();
                    using var connection = new SqlConnection(connectionString);
                    connection.Open();
                    
                    // Test query
                    using var command = new SqlCommand("SELECT @@VERSION", connection);
                    var version = command.ExecuteScalar()?.ToString();
                    string serverInfo = version != null ? version.Substring(0, Math.Min(50, version.Length)) : "Connected";
                    _logger.LogInfo($"SQL Server: {serverInfo}", "database");
                });

                IsDatabaseConnected = true;
                DatabaseStatus = "✅ Connected";
                _logger.LogSuccess("Database connection successful");
            }
            catch (Exception ex)
            {
                IsDatabaseConnected = false;
                DatabaseStatus = "❌ Not Connected";
                _logger.LogError($"Database connection failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void LoadRecentExecutions()
        {
            _logger.LogInfo("Loading recent executions...", "system");
            
            // TODO: Load from persistent storage
            // For now, this is just placeholder
            LastExecutionInfo = "Last run: Never";
        }

        [RelayCommand]
        private async Task RunCompleteETLAsync()
        {
            if (IsBusy)
            {
                _logger.LogWarning("ETL process is already running");
                return;
            }

            try
            {
                IsBusy = true;
                BusyMessage = "Running Complete ETL Process...";
                _progressReporter.Reset();
                
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInfo("═══════════════════════════════════════════", "etl");
                _logger.LogInfo("Starting Complete ETL Process", "etl");
                _logger.LogInfo("═══════════════════════════════════════════", "etl");

                // Create orchestrator with UI logger
                var orchestrator = new ETLOrchestratorWithLogger(_logger, _progressReporter);
                
                // Execute ETL process
                bool success = await Task.Run(() => orchestrator.RunCompleteETLProcessAsync());
                
                stopwatch.Stop();

                if (success)
                {
                    _logger.LogSuccess($"ETL process completed successfully in {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _progressReporter.ReportComplete(true, stopwatch.Elapsed, "All modules executed successfully");
                    
                    // Update execution info
                    LastExecutionInfo = $"Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Success)";
                    TotalFilesProcessed++;
                    
                    // Show success notification
                    Views.MainWindow.NotificationService?.ShowSuccess(
                        "ETL Complete", 
                        $"Process completed successfully in {stopwatch.Elapsed:mm\\:ss}");
                }
                else
                {
                    _logger.LogError($"ETL process completed with errors in {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _progressReporter.ReportComplete(false, stopwatch.Elapsed, "Process completed with errors");
                    LastExecutionInfo = $"Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Failed)";
                    
                    // Show error notification
                    Views.MainWindow.NotificationService?.ShowError(
                        "ETL Failed", 
                        "Process completed with errors. Check logs for details.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ETL execution failed: {ex.Message}");
                _progressReporter.ReportError(ex.Message);
                LastExecutionInfo = $"Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Error)";
                
                // Show error notification
                Views.MainWindow.NotificationService?.ShowError(
                    "ETL Error", 
                    ex.Message);
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void ClearLogs()
        {
            LogEntries.Clear();
            _logger.LogInfo("Log cleared", "system");
        }
    }
}

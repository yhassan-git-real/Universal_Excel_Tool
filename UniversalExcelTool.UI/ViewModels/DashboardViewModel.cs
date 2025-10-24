using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
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

        [ObservableProperty]
        private bool _canRunOperation = true;

        [ObservableProperty]
        private string _operationStatusMessage = string.Empty;

        [ObservableProperty]
        private bool _isOperationRunning;

        [ObservableProperty]
        private bool _canCancelOperation;

        private readonly IUILogger _logger;
        private readonly IProgressReporter _progressReporter;
        private readonly UnifiedConfigurationManager _configManager;
        private readonly IOperationStateService _operationStateService;
        private AvaloniaLogger? _processLogger;
        private CancellationTokenSource? _cancellationTokenSource;

        public DashboardViewModel()
        {
            _logger = new AvaloniaLogger(_logEntries);
            _progressReporter = new AvaloniaProgressReporter(_progressInfo, () => { });
            _configManager = UnifiedConfigurationManager.Instance;
            _operationStateService = OperationStateService.Instance;
            
            // Subscribe to operation state changes
            _operationStateService.OperationStateChanged += OnOperationStateChanged;
            
            // Initialize with current operation state
            var currentOp = _operationStateService.CurrentOperation;
            if (currentOp != null && currentOp.State == OperationState.Running)
            {
                IsOperationRunning = true;
                CanRunOperation = false;
                CanCancelOperation = true;
                OperationStatusMessage = currentOp.GetStatusMessage();
            }
            
            InitializeAsync();
        }

        private void OnOperationStateChanged(object? sender, OperationStateChangedEventArgs e)
        {
            // Ensure UI updates happen on the UI thread
            Dispatcher.UIThread.Post(() =>
            {
                // Update UI based on operation state
                IsOperationRunning = e.Operation != null && e.Operation.State == OperationState.Running;
                CanRunOperation = e.Operation == null || e.Operation.State != OperationState.Running;
                CanCancelOperation = e.Operation != null && e.Operation.State == OperationState.Running;
                
                if (e.Operation != null)
                {
                    OperationStatusMessage = e.Operation.GetStatusMessage();
                }
                else
                {
                    OperationStatusMessage = string.Empty;
                }
            });
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

            // Check if another operation is already running
            if (!_operationStateService.TryStartOperation(OperationType.CompleteETL, "Complete ETL Process"))
            {
                var statusMsg = _operationStateService.GetOperationStatusMessage();
                _logger.LogWarning($"Cannot start operation: {statusMsg}");
                
                // Show notification to user
                Views.MainWindow.NotificationService?.ShowWarning(
                    "Operation Already Running",
                    statusMsg ?? "Another operation is currently in progress. Please wait or cancel the existing operation.");
                return;
            }

            // Create cancellation token for this operation
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                IsBusy = true;
                BusyMessage = "Running Complete ETL Process...";
                _progressReporter.Reset();
                
                // Create file logger for this ETL session
                var logFileName = $"UI_CompleteETL_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var logPath = Path.Combine(_configManager.GetLogFilesPath(), logFileName);
                _processLogger = new AvaloniaLogger(LogEntries, logPath);
                
                var stopwatch = Stopwatch.StartNew();
                _processLogger.LogInfo("═══════════════════════════════════════════", "etl");
                _processLogger.LogInfo($"Starting Complete ETL Process (ID: {_operationStateService.CurrentOperation?.OperationId:N})", "etl");
                _processLogger.LogInfo("═══════════════════════════════════════════", "etl");

                // Create orchestrator with file logger
                var orchestrator = new ETLOrchestratorWithLogger(_processLogger, _progressReporter);
                
                // Execute ETL process with cancellation token
                bool success = await Task.Run(() => orchestrator.RunCompleteETLProcessAsync(null, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
                
                stopwatch.Stop();

                // Check if operation was cancelled
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _processLogger.LogWarning($"ETL process cancelled after {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _progressReporter.ReportComplete(false, stopwatch.Elapsed, "Operation cancelled by user");
                    LastExecutionInfo = $"Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Cancelled)";
                    
                    _operationStateService.CancelOperation();
                    
                    Views.MainWindow.NotificationService?.ShowWarning(
                        "ETL Cancelled", 
                        $"Process was cancelled after {stopwatch.Elapsed:mm\\:ss}");
                    return;
                }

                if (success)
                {
                    _processLogger.LogSuccess($"ETL process completed successfully in {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _processLogger.LogInfo($"Log file saved: {logPath}", "system");
                    _progressReporter.ReportComplete(true, stopwatch.Elapsed, "All modules executed successfully");
                    
                    // Update execution info
                    LastExecutionInfo = $"Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Success)";
                    TotalFilesProcessed++;
                    
                    // Complete operation successfully
                    _operationStateService.CompleteOperation(true, "Process completed successfully");
                    
                    // Show success notification
                    Views.MainWindow.NotificationService?.ShowSuccess(
                        "ETL Complete", 
                        $"Process completed successfully in {stopwatch.Elapsed:mm\\:ss}");
                }
                else
                {
                    _processLogger.LogError($"ETL process completed with errors in {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _processLogger.LogInfo($"Log file saved: {logPath}", "system");
                    _progressReporter.ReportComplete(false, stopwatch.Elapsed, "Process completed with errors");
                    LastExecutionInfo = $"Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Failed)";
                    
                    // Complete operation with failure
                    _operationStateService.CompleteOperation(false, "Process completed with errors");
                    
                    // Show error notification
                    Views.MainWindow.NotificationService?.ShowError(
                        "ETL Failed", 
                        "Process completed with errors. Check logs for details.");
                }
            }
            catch (OperationCanceledException)
            {
                _processLogger?.LogWarning("ETL process was cancelled");
                _progressReporter.ReportError("Operation cancelled");
                LastExecutionInfo = $"Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Cancelled)";
                
                _operationStateService.CancelOperation();
                
                Views.MainWindow.NotificationService?.ShowWarning(
                    "ETL Cancelled", 
                    "Process was cancelled by user");
            }
            catch (Exception ex)
            {
                _processLogger?.LogError($"ETL execution failed: {ex.Message}");
                _progressReporter.ReportError(ex.Message);
                LastExecutionInfo = $"Last run: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (Error)";
                
                // Complete operation with error
                _operationStateService.CompleteOperation(false, $"Error: {ex.Message}");
                
                // Show error notification
                Views.MainWindow.NotificationService?.ShowError(
                    "ETL Error", 
                    ex.Message);
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
                _processLogger?.CloseLog();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        private void CancelOperation()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _logger.LogWarning("Cancelling ETL operation...");
                _cancellationTokenSource.Cancel();
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

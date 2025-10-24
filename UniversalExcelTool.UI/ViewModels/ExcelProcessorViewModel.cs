using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniversalExcelTool.UI.Models;
using UniversalExcelTool.UI.Services;
using UniversalExcelTool.Core;

namespace UniversalExcelTool.UI.ViewModels
{
    /// <summary>
    /// View model for Excel file processing
    /// </summary>
    public partial class ExcelProcessorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<LogEntry> _logEntries = new();

        [ObservableProperty]
        private ProgressInfo _progressInfo = new();

        private readonly IUILogger _logger;
        private readonly IProgressReporter _progressReporter;
        private readonly UnifiedConfigurationManager _configManager;
        private AvaloniaLogger? _processLogger;

        public ExcelProcessorViewModel()
        {
            _logger = new AvaloniaLogger(_logEntries);
            _progressReporter = new AvaloniaProgressReporter(_progressInfo, OnProgressChanged);
            _configManager = UnifiedConfigurationManager.Instance;
            
            _logger.LogInfo("Excel Processor view opened", "processor");
        }

        [RelayCommand]
        private async Task RunExcelProcessorAsync()
        {
            if (IsBusy)
            {
                _logger.LogWarning("Excel Processor is already running");
                return;
            }

            try
            {
                IsBusy = true;
                BusyMessage = "Processing Excel files...";
                _progressReporter.Reset();

                // Create file logger for this session
                var logFileName = $"UI_ExcelProcessor_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var logPath = Path.Combine(_configManager.GetLogFilesPath(), logFileName);
                _processLogger = new AvaloniaLogger(LogEntries, logPath);

                var stopwatch = Stopwatch.StartNew();
                _processLogger.LogInfo("═══════════════════════════════════════════", "processor");
                _processLogger.LogInfo("Starting Excel Processor", "processor");
                _processLogger.LogInfo("═══════════════════════════════════════════", "processor");

                var orchestrator = new ETLOrchestratorWithLogger(_processLogger, _progressReporter);
                var options = new ETLProcessOptions
                {
                    SkipDynamicTableConfig = true
                };

                bool success = await Task.Run(() => orchestrator.RunExcelProcessorAsync(options));

                stopwatch.Stop();

                if (success)
                {
                    _processLogger.LogSuccess($"Excel processing completed in {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _processLogger.LogInfo($"Log file saved: {logPath}", "system");
                    _progressReporter.ReportComplete(true, stopwatch.Elapsed, "Excel files processed successfully");
                }
                else
                {
                    _processLogger.LogError($"Excel processing failed after {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _processLogger.LogInfo($"Log file saved: {logPath}", "system");
                    _progressReporter.ReportComplete(false, stopwatch.Elapsed, "Processing completed with errors");
                }
            }
            catch (Exception ex)
            {
                _processLogger?.LogError($"Excel processing error: {ex.Message}");
                _progressReporter.ReportError(ex.Message);
            }
            finally
            {
                _processLogger?.CloseLog();
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

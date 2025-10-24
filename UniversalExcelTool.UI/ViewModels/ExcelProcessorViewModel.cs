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

                var stopwatch = Stopwatch.StartNew();
                _logger.LogInfo("═══════════════════════════════════════════", "processor");
                _logger.LogInfo("Starting Excel Processor", "processor");
                _logger.LogInfo("═══════════════════════════════════════════", "processor");

                var orchestrator = new ETLOrchestratorWithLogger(_logger, _progressReporter);
                var options = new ETLProcessOptions
                {
                    SkipDynamicTableConfig = true
                };

                bool success = await Task.Run(() => orchestrator.RunExcelProcessorAsync(options));

                stopwatch.Stop();

                if (success)
                {
                    _logger.LogSuccess($"Excel processing completed in {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _progressReporter.ReportComplete(true, stopwatch.Elapsed, "Excel files processed successfully");
                }
                else
                {
                    _logger.LogError($"Excel processing failed after {stopwatch.Elapsed:hh\\:mm\\:ss}");
                    _progressReporter.ReportComplete(false, stopwatch.Elapsed, "Processing completed with errors");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excel processing error: {ex.Message}");
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

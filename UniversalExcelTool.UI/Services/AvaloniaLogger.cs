using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using UniversalExcelTool.UI.Models;

namespace UniversalExcelTool.UI.Services
{
    /// <summary>
    /// Avalonia UI implementation of IUILogger
    /// </summary>
    public class AvaloniaLogger : IUILogger
    {
        private readonly ObservableCollection<LogEntry> _logEntries;
        private readonly int _maxLogEntries;

        public AvaloniaLogger(ObservableCollection<LogEntry> logEntries, int maxLogEntries = 1000)
        {
            _logEntries = logEntries;
            _maxLogEntries = maxLogEntries;
        }

        public void LogInfo(string message, string category = "info")
        {
            AddLogEntry(new LogEntry(message, LogLevel.Info, category));
        }

        public void LogSuccess(string message)
        {
            AddLogEntry(new LogEntry(message, LogLevel.Success, "success"));
        }

        public void LogError(string message)
        {
            AddLogEntry(new LogEntry(message, LogLevel.Error, "error"));
        }

        public void LogWarning(string message)
        {
            AddLogEntry(new LogEntry(message, LogLevel.Warning, "warning"));
        }

        public void LogProgress(string message, long current, long total)
        {
            var progressMessage = $"{message} - {current:N0}/{total:N0} ({(double)current / total * 100:F1}%)";
            AddLogEntry(new LogEntry(progressMessage, LogLevel.Progress, "progress"));
        }

        public void LogDebug(string message)
        {
            AddLogEntry(new LogEntry(message, LogLevel.Debug, "debug"));
        }

        private void AddLogEntry(LogEntry entry)
        {
            // Ensure UI updates happen on UI thread
            Dispatcher.UIThread.Post(() =>
            {
                _logEntries.Add(entry);

                // Limit log entries to prevent memory issues
                while (_logEntries.Count > _maxLogEntries)
                {
                    _logEntries.RemoveAt(0);
                }
            });
        }
    }
}

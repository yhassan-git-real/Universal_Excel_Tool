using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Avalonia.Threading;
using UniversalExcelTool.UI.Models;

namespace UniversalExcelTool.UI.Services
{
    /// <summary>
    /// Avalonia UI implementation of IUILogger with optional file logging
    /// </summary>
    public class AvaloniaLogger : IUILogger
    {
        private readonly ObservableCollection<LogEntry> _logEntries;
        private readonly int _maxLogEntries;
        private readonly string? _logFilePath;
        private readonly StringBuilder _logBuffer;
        private readonly object _fileLock = new object();

        /// <summary>
        /// Creates logger with live UI logs only (no file logging)
        /// </summary>
        public AvaloniaLogger(ObservableCollection<LogEntry> logEntries, int maxLogEntries = 1000)
        {
            _logEntries = logEntries;
            _maxLogEntries = maxLogEntries;
            _logBuffer = new StringBuilder();
        }

        /// <summary>
        /// Creates logger with both live UI logs and file logging
        /// </summary>
        /// <param name="logEntries">ObservableCollection for live UI display</param>
        /// <param name="logFilePath">Full path to log file (e.g., "Logs/UI_Dashboard_20241025_143022.txt")</param>
        /// <param name="maxLogEntries">Maximum in-memory log entries</param>
        public AvaloniaLogger(ObservableCollection<LogEntry> logEntries, string logFilePath, int maxLogEntries = 1000)
        {
            _logEntries = logEntries;
            _maxLogEntries = maxLogEntries;
            _logFilePath = logFilePath;
            _logBuffer = new StringBuilder();

            try
            {
                // Ensure log directory exists
                var directory = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                    
                    // Add diagnostic info to UI logs
                    var diagnosticEntry = new LogEntry($"📁 Log file path: {logFilePath}", LogLevel.Info, "logging");
                    AddLogEntry(diagnosticEntry);
                }

                // Write log header
                WriteToFile($"═══════════════════════════════════════════════════════════");
                WriteToFile($"  Universal Excel Tool - UI Session Log");
                WriteToFile($"  Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                WriteToFile($"  Log File: {logFilePath}");
                WriteToFile($"═══════════════════════════════════════════════════════════\n");
            }
            catch (Exception ex)
            {
                // Show error in UI logs
                var errorEntry = new LogEntry($"⚠️ Failed to initialize log file: {ex.Message}", LogLevel.Warning, "logging");
                AddLogEntry(errorEntry);
            }
        }

        public void LogInfo(string message, string category = "info")
        {
            var entry = new LogEntry(message, LogLevel.Info, category);
            AddLogEntry(entry);
            WriteToFile($"ℹ️  [INFO] {message}");
        }

        public void LogSuccess(string message)
        {
            var entry = new LogEntry(message, LogLevel.Success, "success");
            AddLogEntry(entry);
            WriteToFile($"✅ [SUCCESS] {message}");
        }

        public void LogError(string message)
        {
            var entry = new LogEntry(message, LogLevel.Error, "error");
            AddLogEntry(entry);
            WriteToFile($"❌ [ERROR] {message}");
        }

        public void LogWarning(string message)
        {
            var entry = new LogEntry(message, LogLevel.Warning, "warning");
            AddLogEntry(entry);
            WriteToFile($"⚠️  [WARNING] {message}");
        }

        public void LogProgress(string message, long current, long total)
        {
            var progressMessage = $"{message} - {current:N0}/{total:N0} ({(double)current / total * 100:F1}%)";
            var entry = new LogEntry(progressMessage, LogLevel.Progress, "progress");
            AddLogEntry(entry);
            // Only log progress milestones to file (every 10%)
            var percentage = (double)current / total * 100;
            if (percentage % 10 < 0.1 || current == total)
            {
                WriteToFile($"🔄 [PROGRESS] {progressMessage}");
            }
        }

        public void LogDebug(string message)
        {
            var entry = new LogEntry(message, LogLevel.Debug, "debug");
            AddLogEntry(entry);
            // Debug logs not written to file (too verbose)
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

        private void WriteToFile(string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_fileLock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var logLine = $"[{timestamp}] {message}";
                    _logBuffer.AppendLine(logLine);

                    // Write to file (append mode)
                    File.AppendAllText(_logFilePath, logLine + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // Log error to debug output AND add to UI log collection
                var errorMsg = $"⚠️ Log file write failed: {ex.Message} (Path: {_logFilePath})";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                
                // Add error to UI logs so user can see it
                try
                {
                    AddLogEntry(new LogEntry(errorMsg, LogLevel.Warning, "logging"));
                }
                catch { /* Prevent infinite recursion */ }
            }
        }

        /// <summary>
        /// Writes a session summary and closes the log file
        /// </summary>
        public void CloseLog()
        {
            if (string.IsNullOrEmpty(_logFilePath))
                return;

            WriteToFile($"\n═══════════════════════════════════════════════════════════");
            WriteToFile($"  Session Ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            WriteToFile($"═══════════════════════════════════════════════════════════");
        }
    }
}

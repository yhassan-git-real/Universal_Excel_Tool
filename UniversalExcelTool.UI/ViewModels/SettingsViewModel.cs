using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniversalExcelTool.UI.Models;
using UniversalExcelTool.UI.Services;
using UniversalExcelTool.Core;
using Newtonsoft.Json.Linq;

namespace UniversalExcelTool.UI.ViewModels
{
    /// <summary>
    /// View model for application settings and configuration
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _connectionString = string.Empty;

        [ObservableProperty]
        private string _databaseServer = string.Empty;

        [ObservableProperty]
        private string _databaseName = string.Empty;

        [ObservableProperty]
        private int _authenticationMode = 0; // 0 = Windows, 1 = SQL

        [ObservableProperty]
        private bool _trustServerCertificate = true;

        [ObservableProperty]
        private string _databaseUsername = string.Empty;

        [ObservableProperty]
        private string _databasePassword = string.Empty;

        [ObservableProperty]
        private bool _isSqlAuthentication = false;

        [ObservableProperty]
        private bool _showConnectionNotification = false;

        [ObservableProperty]
        private bool _connectionSuccess = false;

        [ObservableProperty]
        private string _connectionMessage = string.Empty;

        [ObservableProperty]
        private string _inputExcelPath = string.Empty;

        [ObservableProperty]
        private string _outputCsvPath = string.Empty;

        [ObservableProperty]
        private string _logsPath = string.Empty;

        [ObservableProperty]
        private string _dynamicTableManagerPath = string.Empty;

        [ObservableProperty]
        private string _excelProcessorPath = string.Empty;

        [ObservableProperty]
        private string _databaseLoaderPath = string.Empty;

        [ObservableProperty]
        private bool _hasUnsavedChanges;

        [ObservableProperty]
        private ObservableCollection<LogEntry> _logEntries = new();

        private readonly IUILogger _logger;
        private readonly UnifiedConfigurationManager _configManager;
        private readonly string _appSettingsPath;
        private JObject? _currentConfig;

        public SettingsViewModel()
        {
            _logger = new AvaloniaLogger(_logEntries);
            _configManager = UnifiedConfigurationManager.Instance;
            _appSettingsPath = Path.Combine(_configManager.GetRootDirectory(), "appsettings.json");
            
            _logger.LogInfo("Settings view opened", "settings");
            LoadSettings();
        }

        [RelayCommand]
        private void LoadSettings()
        {
            try
            {
                _logger.LogInfo("Loading settings from appsettings.json...", "settings");

                if (!File.Exists(_appSettingsPath))
                {
                    _logger.LogError($"Configuration file not found: {_appSettingsPath}");
                    return;
                }

                string json = File.ReadAllText(_appSettingsPath);
                _currentConfig = JObject.Parse(json);

                // Load database settings from config
                var dbConfig = _currentConfig["Database"];
                if (dbConfig != null)
                {
                    DatabaseServer = dbConfig["Server"]?.ToString() ?? string.Empty;
                    DatabaseName = dbConfig["Database"]?.ToString() ?? string.Empty;
                    bool integratedSecurity = dbConfig["IntegratedSecurity"]?.Value<bool>() ?? true;
                    AuthenticationMode = integratedSecurity ? 0 : 1;
                    DatabaseUsername = dbConfig["Username"]?.ToString() ?? string.Empty;
                    DatabasePassword = dbConfig["Password"]?.ToString() ?? string.Empty;
                    TrustServerCertificate = true; // Default to true for local development
                }

                // Build connection string from components
                BuildConnectionString();

                // Load paths
                var pathsConfig = _currentConfig["Paths"];
                InputExcelPath = pathsConfig?["InputExcelDirectory"]?.ToString() ?? string.Empty;
                OutputCsvPath = pathsConfig?["OutputCsvDirectory"]?.ToString() ?? string.Empty;
                LogsPath = pathsConfig?["LogsDirectory"]?.ToString() ?? string.Empty;

                // Load executable paths
                var modulesConfig = _currentConfig["ExecutableModules"];
                DynamicTableManagerPath = modulesConfig?["DynamicTableManager"]?["ExecutablePath"]?.ToString() ?? string.Empty;
                ExcelProcessorPath = modulesConfig?["ExcelProcessor"]?["ExecutablePath"]?.ToString() ?? string.Empty;
                DatabaseLoaderPath = modulesConfig?["DatabaseLoader"]?["ExecutablePath"]?.ToString() ?? string.Empty;

                HasUnsavedChanges = false;
                _logger.LogSuccess("Settings loaded successfully");
                _logger.LogInfo($"Configuration file: {_appSettingsPath}", "settings");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load settings: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Saving settings...";
                _logger.LogInfo("Saving settings to appsettings.json...", "settings");

                if (_currentConfig == null)
                {
                    _logger.LogError("No configuration loaded");
                    return;
                }

                // Update database configuration
                if (_currentConfig["Database"] != null)
                {
                    _currentConfig["Database"]!["Server"] = DatabaseServer;
                    _currentConfig["Database"]!["Database"] = DatabaseName;
                    _currentConfig["Database"]!["IntegratedSecurity"] = AuthenticationMode == 0;
                    _currentConfig["Database"]!["Username"] = DatabaseUsername;
                    _currentConfig["Database"]!["Password"] = DatabasePassword;
                }

                // Update paths
                if (_currentConfig["Paths"] != null)
                {
                    _currentConfig["Paths"]!["InputExcelDirectory"] = InputExcelPath;
                    _currentConfig["Paths"]!["OutputCsvDirectory"] = OutputCsvPath;
                    _currentConfig["Paths"]!["LogsDirectory"] = LogsPath;
                }

                // Update executable paths
                if (_currentConfig["ExecutableModules"] != null)
                {
                    var modules = _currentConfig["ExecutableModules"];
                    if (modules!["DynamicTableManager"] != null)
                        modules["DynamicTableManager"]!["ExecutablePath"] = DynamicTableManagerPath;
                    if (modules["ExcelProcessor"] != null)
                        modules["ExcelProcessor"]!["ExecutablePath"] = ExcelProcessorPath;
                    if (modules["DatabaseLoader"] != null)
                        modules["DatabaseLoader"]!["ExecutablePath"] = DatabaseLoaderPath;
                }

                await Task.Run(() =>
                {
                    // Create backup
                    string backupPath = _appSettingsPath + ".backup";
                    File.Copy(_appSettingsPath, backupPath, true);
                    _logger.LogInfo($"Backup created: {backupPath}", "settings");

                    // Save updated configuration
                    string updatedJson = _currentConfig.ToString(Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(_appSettingsPath, updatedJson);
                });

                HasUnsavedChanges = false;
                _logger.LogSuccess("Settings saved successfully");
                _logger.LogWarning("⚠️ Restart the application to apply changes");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save settings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }

        [RelayCommand]
        private async Task TestConnectionAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Testing database connection...";
                ShowConnectionNotification = false;
                _logger.LogInfo("Testing connection string...", "database");

                await Task.Run(() =>
                {
                    using var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
                    connection.Open();
                    
                    using var command = new Microsoft.Data.SqlClient.SqlCommand("SELECT @@VERSION", connection);
                    var version = command.ExecuteScalar()?.ToString();
                    string serverInfo = version != null ? version.Substring(0, Math.Min(80, version.Length)) : "Connected";
                    
                    _logger.LogInfo($"SQL Server: {serverInfo}", "database");
                });

                ConnectionSuccess = true;
                ConnectionMessage = "Connection test successful! Database is accessible.";
                ShowConnectionNotification = true;
                _logger.LogSuccess("✅ Connection test successful");
            }
            catch (Exception ex)
            {
                ConnectionSuccess = false;
                ConnectionMessage = $"Connection failed: {ex.Message}";
                ShowConnectionNotification = true;
                _logger.LogError($"❌ Connection test failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                BusyMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            _logger.LogWarning("Resetting to defaults...");
            LoadSettings();
            _logger.LogInfo("Settings reset to last saved values", "settings");
        }

        [RelayCommand]
        private void OpenConfigFile()
        {
            try
            {
                _logger.LogInfo($"Opening configuration file: {_appSettingsPath}", "settings");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _appSettingsPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to open config file: {ex.Message}");
            }
        }

        partial void OnConnectionStringChanged(string value)
        {
            HasUnsavedChanges = true;
        }

        partial void OnInputExcelPathChanged(string value)
        {
            HasUnsavedChanges = true;
        }

        partial void OnOutputCsvPathChanged(string value)
        {
            HasUnsavedChanges = true;
        }

        partial void OnLogsPathChanged(string value)
        {
            HasUnsavedChanges = true;
        }

        partial void OnDynamicTableManagerPathChanged(string value)
        {
            HasUnsavedChanges = true;
        }

        partial void OnExcelProcessorPathChanged(string value)
        {
            HasUnsavedChanges = true;
        }

        partial void OnDatabaseLoaderPathChanged(string value)
        {
            HasUnsavedChanges = true;
        }

        partial void OnDatabaseServerChanged(string value)
        {
            BuildConnectionString();
            HasUnsavedChanges = true;
        }

        partial void OnDatabaseNameChanged(string value)
        {
            BuildConnectionString();
            HasUnsavedChanges = true;
        }

        partial void OnAuthenticationModeChanged(int value)
        {
            IsSqlAuthentication = value == 1;
            BuildConnectionString();
            HasUnsavedChanges = true;
        }

        partial void OnTrustServerCertificateChanged(bool value)
        {
            BuildConnectionString();
            HasUnsavedChanges = true;
        }

        partial void OnDatabaseUsernameChanged(string value)
        {
            BuildConnectionString();
            HasUnsavedChanges = true;
        }

        partial void OnDatabasePasswordChanged(string value)
        {
            BuildConnectionString();
            HasUnsavedChanges = true;
        }

        private void BuildConnectionString()
        {
            if (string.IsNullOrWhiteSpace(DatabaseServer) || string.IsNullOrWhiteSpace(DatabaseName))
            {
                ConnectionString = string.Empty;
                return;
            }

            var builder = new System.Text.StringBuilder();
            builder.Append($"Server={DatabaseServer};");
            builder.Append($"Database={DatabaseName};");

            if (AuthenticationMode == 0) // Windows Authentication
            {
                builder.Append("Integrated Security=true;");
            }
            else // SQL Server Authentication
            {
                if (!string.IsNullOrWhiteSpace(DatabaseUsername))
                {
                    builder.Append($"User Id={DatabaseUsername};");
                    if (!string.IsNullOrWhiteSpace(DatabasePassword))
                    {
                        builder.Append($"Password={DatabasePassword};");
                    }
                }
            }

            if (TrustServerCertificate)
            {
                builder.Append("TrustServerCertificate=true;");
            }

            ConnectionString = builder.ToString();
        }
    }
}

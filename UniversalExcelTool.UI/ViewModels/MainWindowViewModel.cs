using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniversalExcelTool.UI.Services;

namespace UniversalExcelTool.UI.ViewModels
{
    /// <summary>
    /// Main window view model that manages navigation and overall app state
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        private System.Threading.Timer? _clockTimer;

        [ObservableProperty]
        private ViewModelBase? _currentViewModel;

        [ObservableProperty]
        private string _title = "Universal Excel Tool - Modern ETL Manager";

        [ObservableProperty]
        private string _currentPageTitle = "Dashboard";

        [ObservableProperty]
        private string _currentPageIcon = "📊";

        [ObservableProperty]
        private string _currentPageDescription = "Monitor your ETL operations and system status";

        [ObservableProperty]
        private DateTime _currentTime = DateTime.Now;

        [ObservableProperty]
        private string _userName = Environment.UserName;

        public MainWindowViewModel()
        {
            // Start with dashboard view
            NavigateToDashboard();

            // Start clock timer to update current time every second
            _clockTimer = new System.Threading.Timer(_ =>
            {
                CurrentTime = DateTime.Now;
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        [RelayCommand]
        private void NavigateToDashboard()
        {
            CurrentViewModel = new DashboardViewModel();
            CurrentPageIcon = "📊";
            CurrentPageTitle = "Dashboard";
            CurrentPageDescription = "Monitor your ETL operations and system status";
        }

        [RelayCommand]
        private void NavigateToTableConfig()
        {
            CurrentViewModel = new DynamicTableConfigViewModel();
            CurrentPageIcon = "🔧";
            CurrentPageTitle = "Table Configuration";
            CurrentPageDescription = "Configure and manage dynamic database tables";
        }

        [RelayCommand]
        private void NavigateToExcelProcessor()
        {
            CurrentViewModel = new ExcelProcessorViewModel();
            CurrentPageIcon = "📄";
            CurrentPageTitle = "Excel Processor";
            CurrentPageDescription = "Process and transform Excel files";
        }

        [RelayCommand]
        private void NavigateToDatabaseLoader()
        {
            CurrentViewModel = new DatabaseLoaderViewModel();
            CurrentPageIcon = "🗄️";
            CurrentPageTitle = "Database Loader";
            CurrentPageDescription = "Load processed data into database";
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            CurrentViewModel = new SettingsViewModel();
            CurrentPageIcon = "⚙️";
            CurrentPageTitle = "Settings";
            CurrentPageDescription = "Configure application settings and preferences";
        }

        [RelayCommand]
        private async Task ShowAboutAsync()
        {
            var aboutWindow = new Views.AboutWindow();
            
            // Get main window to set as parent
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                await aboutWindow.ShowDialog(desktop.MainWindow!);
            }
        }
    }
}

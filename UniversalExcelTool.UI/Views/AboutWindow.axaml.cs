using Avalonia.Controls;
using Avalonia.Interactivity;

namespace UniversalExcelTool.UI.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.AboutViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

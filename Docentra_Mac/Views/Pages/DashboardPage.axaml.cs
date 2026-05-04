using Avalonia.Controls;

namespace Docentra_Mac.Views.Pages
{
    public partial class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            InitializeComponent();
        }

        private void ToolCard_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
                
                int index = tag switch
                {
                    "Merge" => 1,
                    "Split" => 2,
                    "Watermark" => 3,
                    "Protect" => 4,
                    "Delete" => 5,
                    "PageNumbers" => 6,
                    _ => 0
                };

                mainWindow.NavigateToPage(index);
            }
        }
    }
}

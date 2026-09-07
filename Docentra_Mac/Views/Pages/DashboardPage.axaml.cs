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
                    "DeletePages" => 5,
                    "PageNumbers" => 6,
                    "Crop" => 7,
                    "Sign" => 8,
                    "Convert" => 9,
                    "PdfViewer" => 10,
                    "Compress" => 11,
                    "Metadata" => 12,
                    "Premium" => 13,
                    "Ocr" => 14,
                    _ => 0
                };

                mainWindow.NavigateToPage(index);
            }
        }
    }
}

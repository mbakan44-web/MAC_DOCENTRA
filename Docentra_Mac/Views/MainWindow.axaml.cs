using Avalonia.Controls;
using Docentra_Mac.Services;
using System.Threading.Tasks;

namespace Docentra_Mac.Views
{
    public partial class MainWindow : Window
    {
        private readonly LicenseService _licenseService;
        private readonly UpdateService _updateService;

        public MainWindow()
        {
            InitializeComponent();
            _licenseService = new LicenseService();
            _updateService = new UpdateService();
            
            MainContent.Content = new Pages.DashboardPage();
            NavList.SelectedIndex = 0;
            
            _ = InitializeAppAsync();
        }

        private void NavList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (NavList.SelectedIndex == 0)
            {
                MainContent.Content = new Pages.DashboardPage();
                TxtHeader.Text = (string)this.FindResource("Nav_Dashboard")!;
            }
            else if (NavList.SelectedIndex == 1)
            {
                MainContent.Content = new Pages.MergePage();
                TxtHeader.Text = (string)this.FindResource("Nav_Merge")!;
            }
            else if (NavList.SelectedIndex == 2)
            {
                MainContent.Content = new Pages.SplitPage();
                TxtHeader.Text = (string)this.FindResource("Nav_Split")!;
            }
            else if (NavList.SelectedIndex == 3)
            {
                MainContent.Content = new Pages.WatermarkPage();
                TxtHeader.Text = (string)this.FindResource("Watermark_Title")!;
            }
            else if (NavList.SelectedIndex == 4)
            {
                MainContent.Content = new Pages.ProtectPage();
                TxtHeader.Text = (string)this.FindResource("Protect_Title")!;
            }
            else if (NavList.SelectedIndex == 5)
            {
                MainContent.Content = new Pages.DeletePagesPage();
                TxtHeader.Text = (string)this.FindResource("Delete_Title")!;
            }
            else if (NavList.SelectedIndex == 6)
            {
                MainContent.Content = new Pages.AddPageNumbersPage();
                TxtHeader.Text = (string)this.FindResource("PageNum_Title")!;
            }
            else if (NavList.SelectedIndex == 7)
            {
                MainContent.Content = new Pages.CropPage();
                TxtHeader.Text = (string)this.FindResource("Crop_Title")!;
            }
            else if (NavList.SelectedIndex == 8)
            {
                MainContent.Content = new Pages.SignPage();
                TxtHeader.Text = (string)this.FindResource("Sign_Title")!;
            }
            else if (NavList.SelectedIndex == 9)
            {
                MainContent.Content = new Pages.ConvertPage();
                TxtHeader.Text = (string)this.FindResource("Conv_Title")!;
            }
            else if (NavList.SelectedIndex == 10)
            {
                MainContent.Content = new Pages.PdfViewerPage();
                TxtHeader.Text = (string)this.FindResource("PdfViewer_Title")!;
            }
            else if (NavList.SelectedIndex == 11)
            {
                MainContent.Content = new Pages.CompressPage();
                TxtHeader.Text = (string)this.FindResource("Nav_Compress")!;
            }
            else if (NavList.SelectedIndex == 12)
            {
                MainContent.Content = new Pages.MetadataEditorPage();
                TxtHeader.Text = (string)this.FindResource("Nav_Metadata")!;
            }
            else if (NavList.SelectedIndex == 13)
            {
                MainContent.Content = new Pages.PremiumPage();
                TxtHeader.Text = (string)this.FindResource("Nav_Premium")!;
            }
            else if (NavList.SelectedIndex == 14)
            {
                MainContent.Content = new Pages.OcrPage();
                TxtHeader.Text = (string)this.FindResource("Ocr_Title")!;
            }
        }

        private void Settings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            NavList.SelectedIndex = -1; // Deselect navigation items
            MainContent.Content = new Pages.SettingsPage();
            TxtHeader.Text = (string)this.FindResource("Nav_Settings")!;
        }

        public void NavigateToPage(int index)
        {
            NavList.SelectedIndex = index;
        }

        private async Task InitializeAppAsync()
        {
            // Check License
            var status = await _licenseService.CheckLicenseAsync();
            
            // Check for updates
            var update = await _updateService.CheckForUpdatesAsync();
            if (update.HasUpdate)
            {
                // Handle update notification
            }
        }
    }
}

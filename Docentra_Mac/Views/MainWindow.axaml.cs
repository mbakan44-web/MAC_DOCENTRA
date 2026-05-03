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

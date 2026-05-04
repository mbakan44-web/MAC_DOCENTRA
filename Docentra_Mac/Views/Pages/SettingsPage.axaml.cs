using Avalonia.Controls;
using Docentra_Mac.Services;

namespace Docentra_Mac.Views.Pages
{
    public partial class SettingsPage : UserControl
    {
        private readonly LicenseService _licenseService = new LicenseService();

        public SettingsPage()
        {
            InitializeComponent();
            LoadInfo();
        }

        private async void LoadInfo()
        {
            var status = await _licenseService.CheckLicenseAsync();
            LicenseStatusText.Text = status.IsPremium ? "Premium Active" : "Trial Mode";
            
            // Default selection based on current culture could be added here
        }
    }
}

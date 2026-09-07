using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Docentra_Mac.Services;
using System;

namespace Docentra_Mac.Views.Pages
{
    public partial class PremiumPage : UserControl
    {
        private readonly LicenseService _licenseService = new LicenseService();

        public PremiumPage()
        {
            InitializeComponent();
            LoadLicenseInfo();
        }

        private async void LoadLicenseInfo()
        {
            var status = await _licenseService.CheckLicenseAsync();
            TxtHwid.Text = status.DeviceId;

            if (status.IsPremium)
            {
                TxtLicenseKey.Text = "********-********";
                TxtLicenseKey.IsEnabled = false;
                BtnActivate.IsEnabled = false;
                
                PremiumStatusBox.IsVisible = true;
            }
            else
            {
                PremiumStatusBox.IsVisible = false;
            }
        }

        private async void BtnCopyHwid_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(TxtHwid.Text);
            }
        }

        private void BtnActivate_Click(object? sender, RoutedEventArgs e)
        {
            string key = TxtLicenseKey.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(key)) return;

            BtnActivate.IsEnabled = false;

            bool success = _licenseService.ActivatePremium(key);

            if (success)
            {
                LoadLicenseInfo();
            }
            else
            {
                BtnActivate.IsEnabled = true;
            }
        }
    }
}

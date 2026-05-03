using System.Windows;
using System.Windows.Controls;

namespace PromtAiPdfPro.Views
{
    public partial class ControlCenterPage : Page
    {
        private string _downloadUrl = "https://docentrapdf.com/download";

        public ControlCenterPage()
        {
            InitializeComponent();
            CheckUpdates();
        }

        private async void CheckUpdates()
        {
            var updateService = new Services.UpdateService();
            var (isAvailable, newVersion, downloadUrl) = await updateService.CheckForUpdatesAsync();
            
            if (isAvailable)
            {
                _downloadUrl = downloadUrl;
                var licenseService = new Services.LicenseService();
                var status = await licenseService.CheckLicenseAsync();

                string msg = (string)Application.Current.FindResource("Msg_UpdateAvailable") ?? "New version available";
                string fullMsg = string.Format(msg, newVersion);

                if (status.IsPremium)
                {
                    string premiumMsg = (string)Application.Current.FindResource("Msg_UpdatePremiumFree") ?? "Free for Premium";
                    fullMsg += " " + premiumMsg;
                }

                UpdateInfoBar.Message = fullMsg;
                UpdateInfoBar.IsOpen = true;
            }
        }

        private void BtnDownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_downloadUrl) { UseShellExecute = true });
            }
            catch { }
        }

        private void NavigateTo(System.Type pageType)
        {
            if (Application.Current.MainWindow is MainView mainWindow)
            {
                mainWindow.RootNavigation.Navigate(pageType);
            }
        }

        private void NavigateToMerge_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(MergePage));
        }

        private void NavigateToSplit_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(SplitPage));
        }

        private void NavigateToWord_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(ConvertPage));
        }

        private void NavigateToImageToPdf_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(ConvertPage));
        }

        private void NavigateToOfficeToPdf_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(ConvertPage));
        }

        private void NavigateToOfficeConvert_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(OfficeConvertPage));
        }

        private void NavigateToProtect_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(PasswordSecurityPage));
        }

        private void NavigateToUnlock_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(PasswordSecurityPage));
        }

        private void NavigateToTextWatermark_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(WatermarkPage));
        }

        private void NavigateToLogoWatermark_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(WatermarkPage));
        }

        private void NavigateToOcr_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(OcrPage));
        }

        private void NavigateToCrop_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(CropPage));
        }

        private void NavigateToPageNumbers_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(AddPageNumbersPage));
        }

        private void NavigateToDeletePages_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(DeletePagesPage));
        }

        private void NavigateToSign_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(SignPage));
        }
    }
}

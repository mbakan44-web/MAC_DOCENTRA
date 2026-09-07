using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace PromtAiPdfPro.Views
{
    public partial class ControlCenterPage : Page
    {
        private string _downloadUrl = "https://docentrapdf.com/download";

        public ControlCenterPage()
        {
            InitializeComponent();
            CheckUpdates();
            
            // Uygulamanın en güncel konumunu Windows'a sessizce kaydet
            Task.Run(() => Helpers.FileAssociationHelper.RegisterPdfAssociation());
        }

        private void NavigateToPdfArea_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(typeof(PdfAreaPage));
        }

        // ─── Güncelleme Kontrolü ─────────────────────────────────────────────

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

        // ─── Navigasyon ──────────────────────────────────────────────────────

        private void NavigateTo(System.Type pageType)
        {
            if (Application.Current.MainWindow is MainView mainWindow)
            {
                // Sayfayı bul ve navigasyonu zorla
                mainWindow.RootNavigation.Navigate(pageType);
            }
        }

        private void NavigateToMerge_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(MergePage));
        private void NavigateToSplit_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(SplitPage));
        private void NavigateToWord_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(ConvertPage));
        private void NavigateToImageToPdf_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(ConvertPage));
        private void NavigateToOfficeToPdf_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(ConvertPage));
        private void NavigateToOfficeConvert_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(OfficeConvertPage));
        private void NavigateToProtect_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(PasswordSecurityPage));
        private void NavigateToUnlock_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(PasswordSecurityPage));
        private void NavigateToTextWatermark_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(WatermarkPage));
        private void NavigateToLogoWatermark_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(WatermarkPage));
        private void NavigateToOcr_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(OcrPage));
        private void NavigateToCrop_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(CropPage));
        private void NavigateToCompress_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(CompressPage));
        private void NavigateToPageNumbers_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(AddPageNumbersPage));
        private void NavigateToDeletePages_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(DeletePagesPage));
        private void NavigateToSign_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(SignPage));
        private void NavigateToMetadata_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(MetadataEditorPage));

        private void Card_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && System.IO.Path.GetExtension(files[0]).Equals(".pdf", System.StringComparison.OrdinalIgnoreCase))
                {
                    e.Effects = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void Card_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && System.IO.Path.GetExtension(files[0]).Equals(".pdf", System.StringComparison.OrdinalIgnoreCase))
                {
                    string filePath = files[0];
                    if (sender is FrameworkElement card && card.Tag is string tag)
                    {
                        // Dosya yolunu ve eylemi sakla
                        Helpers.NavigationHelper.PendingFilePath = filePath;
                        Helpers.NavigationHelper.TargetAction = tag;

                        // Etikete göre ilgili sayfaya git
                        switch (tag)
                        {
                            case "Merge": NavigateTo(typeof(MergePage)); break;
                            case "Split": NavigateTo(typeof(SplitPage)); break;
                            case "Sign": NavigateTo(typeof(SignPage)); break;
                            case "DeletePages": NavigateTo(typeof(DeletePagesPage)); break;
                            case "PageNumbers": NavigateTo(typeof(AddPageNumbersPage)); break;
                            case "Crop": NavigateTo(typeof(CropPage)); break;
                            case "Compress": NavigateTo(typeof(CompressPage)); break;
                            case "Protect": NavigateTo(typeof(PasswordSecurityPage)); break;
                            case "Unlock": NavigateTo(typeof(PasswordSecurityPage)); break;
                            case "TextWatermark": NavigateTo(typeof(WatermarkPage)); break;
                            case "LogoWatermark": NavigateTo(typeof(WatermarkPage)); break;
                            case "ImageToPdf": NavigateTo(typeof(ConvertPage)); break;
                            case "OfficeToPdf": NavigateTo(typeof(ConvertPage)); break;
                            case "PdfToWord": NavigateTo(typeof(ConvertPage)); break;
                            case "OfficeConvert": NavigateTo(typeof(OfficeConvertPage)); break;
                            case "Ocr": NavigateTo(typeof(OcrPage)); break;
                            case "Metadata": NavigateTo(typeof(MetadataEditorPage)); break;
                        }
                    }
                }
            }
        }
    }
}

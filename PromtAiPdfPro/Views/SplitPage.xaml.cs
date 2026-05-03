using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class SplitPage : Page
    {
        private PdfService _pdfService = new PdfService();
        private LicenseService _licenseService = new LicenseService();

        public SplitPage()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainView mainWindow)
            {
                mainWindow.RootNavigation.Navigate(typeof(ControlCenterPage));
            }
        }

        private async void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = (string)Application.Current.FindResource("Common_PdfFilter")
            };

            if (dialog.ShowDialog() == true)
            {
                TxtSourceFile.Text = dialog.FileName;
                
                // Show Thumbnail
                var thumbnail = await _pdfService.GetPdfThumbnailAsync(dialog.FileName);
                if (thumbnail != null)
                {
                    ImgPreview.Source = thumbnail;
                    PreviewContainer.Visibility = Visibility.Visible;
                }
                else
                {
                    PreviewContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void BtnSplit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtSourceFile.Text))
            {
                // ... (Warning shown below)
            }
            else
            {
                // Sayfa sınırı kontrolü (14 günden sonra)
                int totalPages = _pdfService.GetPageCount(TxtSourceFile.Text);
                if (!_licenseService.ValidateOperation(totalPages))
                {
                    MessageBox.Show("Free version limit exceeded! After 14 days of trial, you can only process up to 5 pages. Please upgrade to Premium to remove limits.", "Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (Application.Current.MainWindow is MainView mv2) { mv2.RootNavigation.Navigate(typeof(PremiumPage)); }
                    return;
                }
            }

            if (string.IsNullOrEmpty(TxtSourceFile.Text))
            {
                if (Application.Current.MainWindow is MainView mv)
                {
                    mv.SnackbarService.Show(
                        (string)Application.Current.FindResource("Msg_Warning"),
                        (string)Application.Current.FindResource("Msg_SelectFile"),
                        Wpf.Ui.Controls.ControlAppearance.Caution,
                        new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.Warning24),
                        System.TimeSpan.FromSeconds(3)
                    );
                }
                return;
            }

            string range = RbSplitRange.IsChecked == true ? (TxtPageRange.Text ?? string.Empty) : string.Empty;
            bool success = await _pdfService.SplitPagesAsync(TxtSourceFile.Text, range);
            
            if (Application.Current.MainWindow is MainView mainWindow)
            {
                if (success)
                {
                    string dir = System.IO.Path.GetDirectoryName(TxtSourceFile.Text) ?? "";
                    
                    mainWindow.SnackbarService.Show(
                        (string)Application.Current.FindResource("Msg_Success"),
                        (string)Application.Current.FindResource("Msg_SplitSuccess"),
                        Wpf.Ui.Controls.ControlAppearance.Success,
                        new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24),
                        System.TimeSpan.FromSeconds(5)
                    );

                    // Ask to open
                    if (MessageBox.Show((string)Application.Current.FindResource("Split_SuccessWithOpen"), (string)Application.Current.FindResource("Msg_Success"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dir) { UseShellExecute = true });
                    }
                }
                else
                {
                    mainWindow.SnackbarService.Show(
                        (string)Application.Current.FindResource("Msg_Error"),
                        (string)Application.Current.FindResource("Msg_SplitError"),
                        Wpf.Ui.Controls.ControlAppearance.Danger,
                        new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.ErrorCircle24),
                        System.TimeSpan.FromSeconds(5)
                    );
                }
            }
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class WatermarkPage : Page
    {
        private readonly PdfService _pdfService = new PdfService();
        
        private string _textSourcePdf = string.Empty;
        private string _logoSourcePdf = string.Empty;
        private string _selectedLogoPath = string.Empty;

        public WatermarkPage()
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

        #region Text Watermark Flow

        private async void BtnSelectPdfForText_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = (string)Application.Current.FindResource("Common_PdfFilter") };
            if (dialog.ShowDialog() == true)
            {
                _textSourcePdf = dialog.FileName;
                TxtTextSelectedPdf.Text = dialog.SafeFileName;
                TxtTextSelectedPdf.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
                GridTextStep2.IsEnabled = true;

                // Show Thumbnail
                var thumbnail = await _pdfService.GetPdfThumbnailAsync(_textSourcePdf);
                if (thumbnail != null)
                {
                    ImgPreviewText.Source = thumbnail;
                    PreviewContainerText.Visibility = Visibility.Visible;
                }
            }
        }

        private void TxtWatermarkText_TextChanged(object sender, TextChangedEventArgs e)
        {
            StackTextOptions.IsEnabled = !string.IsNullOrEmpty(_textSourcePdf) && !string.IsNullOrEmpty(TxtWatermarkText.Text);
        }

        private async void BtnTextWatermark_Click(object sender, RoutedEventArgs e)
        {
            double pageWidth = 595;
            double pageHeight = 842;

            try
            {
                using (var doc = PdfSharp.Pdf.IO.PdfReader.Open(_textSourcePdf, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
                {
                    if (doc.PageCount > 0)
                    {
                        pageWidth = doc.Pages[0].Width.Point;
                        pageHeight = doc.Pages[0].Height.Point;
                    }
                }
            }
            catch { /* Fallback to A4 */ }

            var posDialog = new TextPositionDialog(TxtWatermarkText.Text, pageWidth, pageHeight);
            posDialog.Owner = Window.GetWindow(this);
            posDialog.ShowDialog();

            if (posDialog.Success)
            {
                var saveDialog = new SaveFileDialog { Filter = (string)Application.Current.FindResource("Common_PdfFilter"), FileName = (string)Application.Current.FindResource("Watermark_TextPrefix") + System.IO.Path.GetFileName(_textSourcePdf) };
                if (saveDialog.ShowDialog() == true)
                {
                    string range = "all";
                    if (RbTextFirst.IsChecked == true) range = "first";
                    if (RbTextCustom.IsChecked == true) range = TxtTextCustomRange.Text;

                    bool result = await _pdfService.AddTextWatermarkAsync(
                        _textSourcePdf, 
                        saveDialog.FileName, 
                        TxtWatermarkText.Text, 
                        posDialog.ResultRect, 
                        posDialog.ResultFontSize, 
                        posDialog.ResultOpacity, 
                        posDialog.ResultRotation,
                        range);

                    if (result)
                    {
                        if (Application.Current.MainWindow is MainView mv)
                        {
                            mv.SnackbarService.Show(
                                (string)Application.Current.FindResource("Msg_Success"),
                                (string)Application.Current.FindResource("Msg_ConvertSuccess"), // Text watermark success reuse
                                Wpf.Ui.Controls.ControlAppearance.Success,
                                new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24),
                                System.TimeSpan.FromSeconds(5)
                            );
                        }

                        if (MessageBox.Show((string)Application.Current.FindResource("Watermark_SuccessWithOpen"), (string)Application.Current.FindResource("Msg_Success"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                        }
                    }
                    else
                    {
                        if (Application.Current.MainWindow is MainView mv)
                        {
                            mv.SnackbarService.Show(
                                (string)Application.Current.FindResource("Msg_Error"),
                                (string)Application.Current.FindResource("Msg_Error"),
                                Wpf.Ui.Controls.ControlAppearance.Danger,
                                new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.ErrorCircle24),
                                System.TimeSpan.FromSeconds(5)
                            );
                        }
                    }
                }
            }
        }

        #endregion

        #region Logo Watermark Flow

        private async void BtnSelectPdfForLogo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = (string)Application.Current.FindResource("Common_PdfFilter") };
            if (dialog.ShowDialog() == true)
            {
                _logoSourcePdf = dialog.FileName;
                TxtLogoSelectedPdf.Text = dialog.SafeFileName;
                TxtLogoSelectedPdf.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
                GridLogoStep2.IsEnabled = true;

                // Show Thumbnail
                var thumbnail = await _pdfService.GetPdfThumbnailAsync(_logoSourcePdf);
                if (thumbnail != null)
                {
                    ImgPreviewLogo.Source = thumbnail;
                    PreviewContainerLogo.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnSelectLogo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = (string)Application.Current.FindResource("Common_ImageFilter") };
            if (dialog.ShowDialog() == true)
            {
                _selectedLogoPath = dialog.FileName;
                TxtSelectedLogoPath.Text = dialog.SafeFileName;
                TxtSelectedLogoPath.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
                StackLogoOptions.IsEnabled = true;
            }
        }

        private async void BtnImageWatermark_Click(object sender, RoutedEventArgs e)
        {
            double pageWidth = 595;
            double pageHeight = 842;

            try
            {
                using (var doc = PdfSharp.Pdf.IO.PdfReader.Open(_logoSourcePdf, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
                {
                    if (doc.PageCount > 0)
                    {
                        pageWidth = doc.Pages[0].Width.Point;
                        pageHeight = doc.Pages[0].Height.Point;
                    }
                }
            }
            catch { /* Fallback to A4 */ }

            var posDialog = new LogoPositionDialog(_selectedLogoPath, pageWidth, pageHeight);
            posDialog.Owner = Window.GetWindow(this);
            posDialog.ShowDialog();

            if (posDialog.Success)
            {
                var saveDialog = new SaveFileDialog { Filter = (string)Application.Current.FindResource("Common_PdfFilter"), FileName = (string)Application.Current.FindResource("Watermark_LogoPrefix") + System.IO.Path.GetFileName(_logoSourcePdf) };
                if (saveDialog.ShowDialog() == true)
                {
                    string range = "all";
                    if (RbLogoFirst.IsChecked == true) range = "first";
                    if (RbLogoCustom.IsChecked == true) range = TxtLogoCustomRange.Text;

                    bool result = await _pdfService.AddImageWatermarkAsync(
                        _logoSourcePdf, 
                        saveDialog.FileName, 
                        _selectedLogoPath, 
                        posDialog.ResultRect, 
                        posDialog.ResultOpacity, 
                        range);

                    if (result)
                    {
                        if (Application.Current.MainWindow is MainView mv)
                        {
                            mv.SnackbarService.Show(
                                (string)Application.Current.FindResource("Msg_Success"),
                                (string)Application.Current.FindResource("Msg_ConvertSuccess"),
                                Wpf.Ui.Controls.ControlAppearance.Success,
                                new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24),
                                System.TimeSpan.FromSeconds(5)
                            );
                        }

                        if (MessageBox.Show((string)Application.Current.FindResource("Watermark_SuccessWithOpen"), (string)Application.Current.FindResource("Msg_Success"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                        }
                    }
                    else
                    {
                        if (Application.Current.MainWindow is MainView mv)
                        {
                            mv.SnackbarService.Show(
                                (string)Application.Current.FindResource("Msg_Error"),
                                (string)Application.Current.FindResource("Msg_Error"),
                                Wpf.Ui.Controls.ControlAppearance.Danger,
                                new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.ErrorCircle24),
                                System.TimeSpan.FromSeconds(5)
                            );
                        }
                    }
                }
            }
        }

        #endregion

        #region Helpers

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string resourceKey)
            {
                var helpText = (string)Application.Current.FindResource(resourceKey);
                var title = (string)Application.Current.FindResource("Nav_Watermark");
                MessageBox.Show(helpText, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRangeHelp_Click(object sender, RoutedEventArgs e)
        {
            var helpText = (string)Application.Current.FindResource("Sec_RangeHelp");
            MessageBox.Show(helpText, (string)Application.Current.FindResource("Sec_RangeHelpTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}

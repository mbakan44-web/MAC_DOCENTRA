using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PromtAiPdfPro.Services;
using System.Diagnostics;

namespace PromtAiPdfPro.Views
{
    public partial class CompressPage : Page
    {
        private PdfService _pdfService = new PdfService();
        private string? _selectedFile;
        private string _compressionLevel = "Low";

        public CompressPage()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (!string.IsNullOrEmpty(Helpers.NavigationHelper.PendingFilePath))
                {
                    string path = Helpers.NavigationHelper.PendingFilePath;
                    Helpers.NavigationHelper.PendingFilePath = null; // Temizle
                    
                    _selectedFile = path;
                    TxtSourceFile.Text = Path.GetFileName(_selectedFile);
                    
                    var thumb = await _pdfService.GetPdfThumbnailAsync(_selectedFile);
                    if (thumb != null)
                    {
                        ImgPreview.Source = thumb;
                        PreviewContainer.Visibility = Visibility.Visible;
                    }
                }
            };
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private async void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = Application.Current.Resources["Common_PdfFilter"].ToString()
            };

            if (openDialog.ShowDialog() == true)
            {
                _selectedFile = openDialog.FileName;
                TxtSourceFile.Text = Path.GetFileName(_selectedFile);

                // Show Preview
                var thumb = await _pdfService.GetPdfThumbnailAsync(_selectedFile);
                if (thumb != null)
                {
                    ImgPreview.Source = thumb;
                    PreviewContainer.Visibility = Visibility.Visible;
                }
            }
        }

        private void CardLevel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.CardAction card && card.Tag is string level)
            {
                _compressionLevel = level;
                UpdateLevelUI();
            }
        }

        private void UpdateLevelUI()
        {
            // Reset all
            CardLow.BorderThickness = new Thickness(0);
            CardMed.BorderThickness = new Thickness(0);
            CardHigh.BorderThickness = new Thickness(0);
            IconLow.Visibility = Visibility.Collapsed;
            IconMed.Visibility = Visibility.Collapsed;
            IconHigh.Visibility = Visibility.Collapsed;

            // Set active
            var accent = (System.Windows.Media.Brush)Application.Current.Resources["PremiumAccentBrush"];
            switch (_compressionLevel)
            {
                case "Low":
                    CardLow.BorderThickness = new Thickness(2);
                    CardLow.BorderBrush = accent;
                    IconLow.Visibility = Visibility.Visible;
                    break;
                case "Medium":
                    CardMed.BorderThickness = new Thickness(2);
                    CardMed.BorderBrush = accent;
                    IconMed.Visibility = Visibility.Visible;
                    break;
                case "High":
                    CardHigh.BorderThickness = new Thickness(2);
                    CardHigh.BorderBrush = accent;
                    IconHigh.Visibility = Visibility.Visible;
                    break;
            }
        }

        private async void BtnCompress_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile))
            {
                MessageBox.Show(Application.Current.Resources["Msg_SelectFile"].ToString(), 
                                Application.Current.Resources["Msg_Warning"].ToString(), 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = Application.Current.Resources["Common_PdfFilter"].ToString(),
                FileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_Compressed.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                BtnCompress.IsEnabled = false;
                
                bool success = await _pdfService.CompressPdfAsync(_selectedFile, saveDialog.FileName, _compressionLevel);
                
                BtnCompress.IsEnabled = true;

                if (success)
                {
                    var result = MessageBox.Show(Application.Current.Resources["Compress_SuccessWithOpen"].ToString(),
                                                 Application.Current.Resources["Msg_Success"].ToString(),
                                                 MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                    }
                }
                else
                {
                    MessageBox.Show(Application.Current.Resources["Msg_ProcessError"].ToString(),
                                    Application.Current.Resources["Msg_Error"].ToString(),
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

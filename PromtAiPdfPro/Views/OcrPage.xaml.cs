using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;
using PromtAiPdfPro.Services;

namespace PromtAiPdfPro.Views
{
    public partial class OcrPage : Page
    {
        private readonly PdfService _pdfService = new PdfService();
        private string _selectedPath = string.Empty;

        public OcrPage()
        {
            InitializeComponent();
            LoadOcrLanguages();
        }

        private void LoadOcrLanguages()
        {
            // Sisteme yüklü OCR dillerini al
            var languages = OcrEngine.AvailableRecognizerLanguages;
            if (languages.Count == 0)
            {
                MessageBox.Show((string)Application.Current.FindResource("Msg_OcrNoLang"), (string)Application.Current.FindResource("Msg_Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CboLanguages.ItemsSource = languages;
            
            // Varsayılan dil seçimi
            var turkish = languages.FirstOrDefault(l => l.LanguageTag.StartsWith("tr", StringComparison.OrdinalIgnoreCase));
            var english = languages.FirstOrDefault(l => l.LanguageTag.StartsWith("en", StringComparison.OrdinalIgnoreCase));

            CboLanguages.SelectedItem = turkish ?? english ?? languages.FirstOrDefault();
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
                Filter = (string)Application.Current.FindResource("Ocr_Filter")
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedPath = dialog.FileName;
                TxtSourcePath.Text = Path.GetFileName(_selectedPath);
                BtnRunOcr.IsEnabled = true;

                // Preview
                if (_selectedPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var thumb = await _pdfService.GetPdfThumbnailAsync(_selectedPath);
                    ImgPreview.Source = thumb;
                }
                else
                {
                    ImgPreview.Source = new BitmapImage(new Uri(_selectedPath));
                }
                PreviewContainer.Visibility = Visibility.Visible;
            }
        }

        private async void BtnRunOcr_Click(object sender, RoutedEventArgs e)
        {
            if (CboLanguages.SelectedItem is not Windows.Globalization.Language selectedLang) return;

            GridLoading.Visibility = Visibility.Visible;
            TxtResult.Text = string.Empty;

            try
            {
                OcrEngine engine = OcrEngine.TryCreateFromLanguage(selectedLang);
                if (engine == null)
                {
                    MessageBox.Show((string)Application.Current.FindResource("Msg_OcrEngineError"));
                    return;
                }

                string resultText = "";

                if (_selectedPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    // PDF İşleme
                    StorageFile file = await StorageFile.GetFileFromPathAsync(_selectedPath);
                    var pdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);

                    for (uint i = 0; i < pdfDoc.PageCount; i++)
                    {
                        using (var page = pdfDoc.GetPage(i))
                        {
                            using (var stream = new InMemoryRandomAccessStream())
                            {
                                await page.RenderToStreamAsync(stream);
                                resultText += await PerformOcrOnStreamAsync(engine, stream) + "\n\n--- " + (string)Application.Current.FindResource("Common_Page") + " " + (i + 1) + " ---\n\n";
                            }
                        }
                    }
                }
                else
                {
                    // Resim İşleme
                    StorageFile imgFile = await StorageFile.GetFileFromPathAsync(_selectedPath);
                    using (IRandomAccessStream stream = await imgFile.OpenAsync(FileAccessMode.Read))
                    {
                        resultText = await PerformOcrOnStreamAsync(engine, stream);
                    }
                }

                TxtResult.Text = resultText;

                if (Application.Current.MainWindow is MainView mv)
                {
                    mv.SnackbarService.Show((string)Application.Current.FindResource("Msg_Success"), (string)Application.Current.FindResource("Msg_OcrSuccess"), Wpf.Ui.Controls.ControlAppearance.Success, new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24), TimeSpan.FromSeconds(3));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show((string)Application.Current.FindResource("Msg_Error") + ": " + ex.Message);
            }
            finally
            {
                GridLoading.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<string> PerformOcrOnStreamAsync(OcrEngine engine, IRandomAccessStream stream)
        {
            Windows.Graphics.Imaging.BitmapDecoder decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();
            
            OcrResult result = await engine.RecognizeAsync(bitmap);
            return result.Text;
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtResult.Text))
            {
                Clipboard.SetText(TxtResult.Text);
                if (Application.Current.MainWindow is MainView mv)
                {
                    mv.SnackbarService.Show((string)Application.Current.FindResource("Msg_Copied"), (string)Application.Current.FindResource("Msg_CopiedDesc"), Wpf.Ui.Controls.ControlAppearance.Info, new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.Copy24), TimeSpan.FromSeconds(2));
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtResult.Text)) return;

            var dialog = new SaveFileDialog 
            { 
                Filter = (string)Application.Current.FindResource("Common_TxtFilter"), 
                FileName = Path.GetFileNameWithoutExtension(_selectedPath) + "_OCR.txt" 
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, TxtResult.Text);
                
                var title = (string)Application.Current.FindResource("Msg_Success");
                var msg = (string)Application.Current.FindResource("Ocr_SuccessWithOpen");

                if (MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
        }
    }
}

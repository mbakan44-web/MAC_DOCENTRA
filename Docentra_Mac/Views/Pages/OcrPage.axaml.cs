using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Docentra_Mac.Services;
using System;
using System.IO;
using System.Linq;

namespace Docentra_Mac.Views.Pages
{
    public partial class OcrPage : UserControl
    {
        private readonly OcrService _ocrService = new OcrService();
        private readonly PdfService _pdfService = new PdfService();
        private string? _selectedFile;

        public OcrPage()
        {
            InitializeComponent();
            LoadOcrLanguages();
        }

        private void LoadOcrLanguages()
        {
            var languages = _ocrService.GetAvailableLanguages();
            CboLanguages.ItemsSource = languages;

            // Default selection: Turkish first, English second, or first available
            var tr = languages.FirstOrDefault(l => l.LanguageTag.StartsWith("tr", StringComparison.OrdinalIgnoreCase));
            var en = languages.FirstOrDefault(l => l.LanguageTag.StartsWith("en", StringComparison.OrdinalIgnoreCase));
            
            CboLanguages.SelectedItem = tr ?? en ?? languages.FirstOrDefault();
        }

        private async void BtnBrowse_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var customFilter = new FilePickerFileType("Görseller ve PDF Belgeleri")
            {
                Patterns = new[] { "*.pdf", "*.png", "*.jpg", "*.jpeg" }
            };

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Görsel veya PDF Dosyası Seçin",
                FileTypeFilter = new[] { customFilter, FilePickerFileTypes.Pdf },
                AllowMultiple = false
            });

            if (files.Count > 0)
            {
                _selectedFile = files[0].Path.LocalPath;
                TxtSourceFile.Text = Path.GetFileName(_selectedFile);

                // Update UI details and enable the run button
                BtnRunOcr.IsEnabled = true;
                UpdatePreviewUI();
            }
        }

        private void UpdatePreviewUI()
        {
            if (string.IsNullOrEmpty(_selectedFile))
            {
                PreviewBox.IsVisible = false;
                return;
            }

            TxtFileName.Text = Path.GetFileName(_selectedFile);
            string ext = Path.GetExtension(_selectedFile).ToLower();

            if (ext == ".pdf")
            {
                TxtFileType.Text = "PDF Belgesi";
                PreviewIcon.Data = (StreamGeometry)this.FindResource("DocumentRegular")!;
                PreviewIcon.Foreground = new SolidColorBrush(Color.Parse("#EF4444")); // Red-ish for PDF
            }
            else
            {
                TxtFileType.Text = "Görsel Dosyası";
                PreviewIcon.Data = (StreamGeometry)this.FindResource("ImageIconRegular")!;
                PreviewIcon.Foreground = new SolidColorBrush(Color.Parse("#10B981")); // Emerald-ish for images
            }

            PreviewBox.IsVisible = true;
        }

        private async void BtnRunOcr_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFile) || CboLanguages.SelectedItem is not OcrLanguage selectedLanguage)
            {
                return;
            }

            // UI feedback
            BtnRunOcr.IsEnabled = false;
            BtnBrowse.IsEnabled = false;
            CboLanguages.IsEnabled = false;
            PanelLoading.IsVisible = true;
            TxtResult.Text = string.Empty;
            
            BtnCopy.IsEnabled = false;
            BtnSave.IsEnabled = false;

            try
            {
                // Run background OCR
                string textResult = await _ocrService.RecognizeTextAsync(_selectedFile, selectedLanguage.LanguageTag);

                TxtResult.Text = textResult;

                // Enable copy/save if text was successfully recognized
                if (!string.IsNullOrWhiteSpace(textResult))
                {
                    BtnCopy.IsEnabled = true;
                    BtnSave.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                TxtResult.Text = $"Hata oluştu:\n{ex.Message}";
            }
            finally
            {
                BtnRunOcr.IsEnabled = true;
                BtnBrowse.IsEnabled = true;
                CboLanguages.IsEnabled = true;
                PanelLoading.IsVisible = false;
            }
        }

        private async void BtnCopy_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtResult.Text)) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null && topLevel.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(TxtResult.Text);
            }
        }

        private async void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtResult.Text) || string.IsNullOrEmpty(_selectedFile)) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "OCR Sonucunu Kaydet",
                DefaultExtension = "txt",
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(Path.GetDirectoryName(_selectedFile)!)),
                SuggestedFileName = Path.GetFileNameWithoutExtension(_selectedFile) + "_OCR.txt",
                FileTypeChoices = new[] { FilePickerFileTypes.TextPlain }
            });

            if (file != null)
            {
                try
                {
                    await File.WriteAllTextAsync(file.Path.LocalPath, TxtResult.Text);
                    _pdfService.OpenFile(file.Path.LocalPath);
                }
                catch (Exception ex)
                {
                    TxtResult.Text += $"\n\n[Dosya Kaydetme Hatası: {ex.Message}]";
                }
            }
        }
    }
}
